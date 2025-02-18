namespace FclEx.Serilog;

public partial class JsonFormatter : ITextFormatter
{
    protected const string LinkingMetadataKey = "newrelic.linkingmetadata";

    protected JsonFormatterOptions Options { get; }
    protected ExceptionFormatOptions ExceptionOptions => Options.ExceptionFormatOptions;

    public JsonFormatter(JsonFormatterOptions? options = null)
    {
        Options = options ?? JsonFormatterOptions.Default;
        Options.ExceptionFormatOptions ??= ExceptionFormatOptions.Default;
    }

    /// <summary>
    /// Format the log event into the output. Subsequent events will be newline-delimited.
    /// </summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="output">The output.</param>
    public virtual void Format(LogEvent logEvent, TextWriter output)
    {
        output.Write('{');
        WriteBasicData(logEvent, output);
        WriteProperties(logEvent, output);
        output.Write('}');
        output.WriteLine();
    }

    protected virtual void WriteBasicData(LogEvent logEvent, TextWriter output)
    {
        var time = logEvent.Timestamp.UtcDateTime.ToString("O");
        var message = logEvent.RenderMessage("l");

        WriteJsonData(Options.UtcTimeName, time, output, false);
        WriteJsonData(Options.LogLevelName, logEvent.Level.ToString(), output, true);
        WriteJsonData(Options.MessageName, message, output, true);

        if (logEvent.Exception is not { } ex)
            return;

        WriteException(message, ex, output);
    }

    protected virtual void WriteException(string logEventMessage, Exception ex, TextWriter output)
    {
        try
        {
            if (Options.EnableExceptionFormat)
            {
                WriteFormattedException(logEventMessage, ex, output);
            }
            else
            {
                WriteJsonData(Options.ExceptionName, ex.ToString(), output, true);
            }

            PrintException(ex);
        }
        catch (Exception e)
        {
            WriteJsonData(Options.ExceptionName, "[Failed to Format Exception]" + e.Message, output, true);
        }
    }

    protected virtual void WriteProperties(LogEvent logEvent, TextWriter output)
    {
        foreach (var (key, value) in logEvent.Properties)
        {
            if (key == LinkingMetadataKey)
            {
                WriteLinkingMetadataProperties(value, output);
                continue;
            }

            var name = key;
            if (name.Length > 0 && name[0] == '@')
            {
                // Escape first '@' by doubling
                name = '@' + name;
            }

            WriteFormattedJsonData(name, value, output);
        }
    }

    protected virtual void WriteLinkingMetadataProperties(LogEventPropertyValue nrPropValues, TextWriter output)
    {
        if (nrPropValues is not DictionaryValue dictionaryValue)
            return;

        foreach (var (key, value) in dictionaryValue.Elements)
        {
            WriteFormattedJsonData(key.Value?.ToString(), value, output);
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
                        output.Write("\\\\");
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
                output.Write(str.Substring(cleanSegmentStart));
        }
        else
        {
            output.Write(str);
        }
    }
}