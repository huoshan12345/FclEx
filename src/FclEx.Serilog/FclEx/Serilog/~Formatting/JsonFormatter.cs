namespace FclEx.Serilog;

public partial class JsonFormatter : ITextFormatter
{
    protected const string LinkingMetadataKey = "newrelic.linkingmetadata";

    protected JsonFormatterOptions Options { get; }
    protected ExceptionFormatOptions ExceptionOptions => Options.ExceptionFormatOptions;

    /// <summary>Creates a formatter with the supplied options or a new, independent set of defaults.</summary>
    /// <param name="options">Options retained by this formatter, or null to create independent defaults.</param>
    /// <remarks>Complete configuration before concurrent formatting. Explicitly sharing options also shares subsequent changes.</remarks>
    public JsonFormatter(JsonFormatterOptions? options = null)
    {
        Options = options ?? new JsonFormatterOptions();
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        Options.ExceptionFormatOptions ??= new ExceptionFormatOptions();
    }

    /// <summary>
    /// Writes one JSON object followed by a newline, omitting fields whose configured names are empty.
    /// Event and linking-metadata properties whose final JSON names match a configured basic field are omitted.
    /// </summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="output">The output.</param>
    /// <exception cref="InvalidOperationException">Basic field names are not distinct, or duplicate properties are encountered under the Throw policy.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The property collision policy is not defined.</exception>
    public virtual void Format(LogEvent logEvent, TextWriter output)
    {
        ValidateBasicFieldNames();
        if (Options.PropertyCollisionPolicy is not (JsonPropertyCollisionPolicy.KeepFirst or JsonPropertyCollisionPolicy.KeepLast or JsonPropertyCollisionPolicy.Throw))
            throw new ArgumentOutOfRangeException(nameof(JsonFormatterOptions.PropertyCollisionPolicy), Options.PropertyCollisionPolicy, "Unknown property collision policy.");
        // Omitting the first field leaves a leading separator in the existing field writers.
        // Buffer this case so the first separator can be removed before the members are written.
        // Throw mode buffers the event so a property conflict cannot leave a partial JSON object in the output.
        using var buffer = Options.UtcTimeName.IsNullOrEmpty() || Options.PropertyCollisionPolicy == JsonPropertyCollisionPolicy.Throw
            ? StringBuilder.GetCached() : null;
        using var bufferWriter = buffer == null ? null : new StringWriter(buffer.Value, output.FormatProvider);
        var writer = bufferWriter ?? output;

        if (buffer == null)
            output.Write('{');
        WriteBasicData(logEvent, writer);
        WriteProperties(logEvent, writer);
        if (buffer != null)
        {
            output.Write('{');
            var builder = buffer.Value;
            if (builder.Length > 0 && builder[0] == ',')
                builder.Remove(0, 1);
            output.Write(builder.ToString());
        }
        output.Write('}');
        output.WriteLine();
    }

    private void ValidateBasicFieldNames()
    {
        // Options are mutable, so validate before each event and before writing any JSON.
        if (Collides(Options.UtcTimeName, Options.LogLevelName)
            || Collides(Options.UtcTimeName, Options.MessageName)
            || Collides(Options.UtcTimeName, Options.ExceptionName)
            || Collides(Options.LogLevelName, Options.MessageName)
            || Collides(Options.LogLevelName, Options.ExceptionName)
            || Collides(Options.MessageName, Options.ExceptionName))
            throw new InvalidOperationException("Nonempty JSON basic field names must be distinct.");

        static bool Collides(string? first, string? second) => !string.IsNullOrEmpty(first) && first == second;
    }

    protected virtual void WriteBasicData(LogEvent logEvent, TextWriter output)
    {
        var time = logEvent.Timestamp.UtcDateTime.ToString("O");
        var message = logEvent.RenderMessage("l");

        WriteJsonData(Options.UtcTimeName, time, output, false);
        WriteJsonData(Options.LogLevelName, logEvent.Level.ToString(), output, true);
        WriteJsonData(Options.MessageName, message, output, true);

        if (logEvent.GetOriginalException() is not { } ex)
            return;

        WriteException(message, ex, output);
    }

    /// <summary>Writes an exception member, using a diagnostic string if exception formatting fails.</summary>
    /// <param name="logEventMessage">The rendered message used to identify duplicate exception text.</param>
    /// <param name="ex">The exception to format.</param>
    /// <param name="output">The writer receiving a complete comma-prefixed exception member.</param>
    /// <remarks>An empty exception member name omits formatting. Exception fragments are buffered before being committed to the output.</remarks>
    protected virtual void WriteException(string logEventMessage, Exception ex, TextWriter output)
    {
        if (Options.ExceptionName.IsNullOrEmpty())
            return;

        // Commit the exception member only after formatting succeeds, or replace the whole fragment.
        using var buffer = StringBuilder.GetCached();
        using var writer = new StringWriter(buffer.Value, output.FormatProvider);
        try
        {
            if (Options.EnableExceptionFormat)
            {
                WriteFormattedException(logEventMessage, ex, writer);
            }
            else
            {
                WriteJsonData(Options.ExceptionName, ex.ToString(), writer, true);
            }
        }
        catch (Exception e)
        {
            buffer.Value.Clear();
            WriteJsonData(Options.ExceptionName, "[Failed to Format Exception]" + e.Message, writer, true);
        }
        output.Write(buffer.Value.ToString());
    }

    /// <summary>Resolves final property names and collisions, then writes each selected value once.</summary>
    /// <param name="logEvent">The event whose top-level and linking-metadata properties are collected.</param>
    /// <param name="output">The writer receiving comma-prefixed JSON members.</param>
    protected virtual void WriteProperties(LogEvent logEvent, TextWriter output)
    {
        var properties = new List<KeyValuePair<string, LogEventPropertyValue>>();
        var positions = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var (key, value) in GetPropertyCandidates(logEvent))
        {
            if (key.IsNullOrEmpty())
                continue;

            var name = Options.PropertyNamingPolicy.ConvertName(key);
            // Reserve configured basic names even when this event has no exception.
            if (name == Options.UtcTimeName || name == Options.LogLevelName || name == Options.MessageName || name == Options.ExceptionName)
                continue;

            if (positions.TryGetValue(name, out var position))
            {
                switch (Options.PropertyCollisionPolicy)
                {
                    case JsonPropertyCollisionPolicy.KeepFirst:
                        continue;
                    case JsonPropertyCollisionPolicy.KeepLast:
                        properties[position] = new(name, value);
                        continue;
                    case JsonPropertyCollisionPolicy.Throw:
                        throw new InvalidOperationException($"Multiple log properties map to the JSON name '{name}'.");
                }
            }

            positions.Add(name, properties.Count);
            properties.Add(new(name, value));
        }

        foreach (var (name, value) in properties)
            WriteFormattedJsonData(name, value, output);
    }

    /// <summary>Enumerates event properties in their original order, expanding linking metadata at its original position.</summary>
    /// <param name="logEvent">The source event.</param>
    /// <returns>Names after event-property @ escaping but before the naming policy, together with their values.</returns>
    /// <remarks>Override this method to customize candidate properties before collision resolution.</remarks>
    protected virtual IEnumerable<KeyValuePair<string, LogEventPropertyValue>> GetPropertyCandidates(LogEvent logEvent)
    {
        foreach (var (key, value) in logEvent.Properties)
        {
            if (key == LinkingMetadataKey)
            {
                if (value is DictionaryValue metadata)
                {
                    foreach (var (metadataKey, metadataValue) in metadata.Elements)
                        yield return new(metadataKey.Value?.ToString() ?? "", metadataValue);
                }
                continue;
            }

            var name = key;
            if (name.Length > 0 && name[0] == '@')
            {
                // Escape first '@' by doubling
                name = '@' + name;
            }

            yield return new(name, value);
        }
    }

    protected virtual void WriteJsonData(string? key, string value, TextWriter output, bool writePrefixComma)
    {
        if (key.IsNullOrEmpty())
            return;

        if (writePrefixComma)
            output.Write(',');

        JsonValueFormatter.WriteQuotedJsonString(key, output);
        output.Write(':');
        JsonValueFormatter.WriteQuotedJsonString(value, output);
    }

    /// <summary>Writes a selected property whose final name has already passed naming, reserved-field, and collision checks.</summary>
    /// <param name="key">The final JSON member name; this method does not apply a naming policy again.</param>
    /// <param name="value">The selected structured property value.</param>
    /// <param name="output">The writer receiving a comma-prefixed JSON member.</param>
    protected virtual void WriteFormattedJsonData(string? key, LogEventPropertyValue value, TextWriter output)
    {
        if (key.IsNullOrEmpty())
            return;

        output.Write(',');
        JsonValueFormatter.WriteQuotedJsonString(key, output);
        output.Write(':');
        Options.ValueFormatter.Format(value, output);
    }

    /// <summary>
    /// Write a valid non-quoted JSON string literal, escaping as necessary.
    /// </summary>
    /// <param name="str">The string value to write.</param>
    /// <param name="output">The output.</param>
    public static void WriteJsonString(string str, TextWriter output)
    {
        var cleanSegmentStart = 0;
        var anyEscaped = false;

        for (var i = 0; i < str.Length; ++i)
        {
            var c = str[i];
            if (c is < (char)32 or '\\' or '"')
            {
                anyEscaped = true;

                output.Write(str.Substring(cleanSegmentStart, i - cleanSegmentStart));
                cleanSegmentStart = i + 1;

                switch (c)
                {
                    case '"':
                    {
                        output.Write("\\\"");
                        break;
                    }
                    case '\\':
                    {
                        output.Write(@"\\");
                        break;
                    }
                    case '\n':
                    {
                        output.Write("\\n");
                        break;
                    }
                    case '\r':
                    {
                        output.Write("\\r");
                        break;
                    }
                    case '\f':
                    {
                        output.Write("\\f");
                        break;
                    }
                    case '\t':
                    {
                        output.Write("\\t");
                        break;
                    }
                    default:
                    {
                        output.Write("\\u");
                        output.Write(((int)c).ToString("X4"));
                        break;
                    }
                }
            }
        }

        if (anyEscaped)
        {
            if (cleanSegmentStart != str.Length)
                output.Write(str[cleanSegmentStart..]);
        }
        else
        {
            output.Write(str);
        }
    }
}
