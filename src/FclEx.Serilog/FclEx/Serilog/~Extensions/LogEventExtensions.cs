using FclEx.Web;
using Serilog.Parsing;

namespace FclEx.Serilog;

/// <summary>Renders, mutates, queries, and normalizes structured Serilog events.</summary>
/// <remarks>Mutation helpers update the supplied event in place; all consumers holding that event observe the changes.</remarks>
public static class LogEventExtensions
{
    /// <summary>Renders the message template with Serilog's message-token formatting rules.</summary>
    /// <param name="logEvent">The event supplying the template and properties.</param>
    /// <param name="output">Receives the rendered message without an added newline.</param>
    /// <param name="format">Serilog message-token formatting, such as l for literal strings or j for JSON values, or null for defaults.</param>
    /// <param name="formatProvider">Formats property values, or null to use Serilog's default provider.</param>
    public static void RenderMessage(this LogEvent logEvent, TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
    {
#if NET8_0_OR_GREATER
        UnsafeAccessors.MessageTemplateRenderer_Render(null, logEvent.MessageTemplate, logEvent.Properties, output, format, formatProvider);
#else
        MethodInfos.MessageTemplateRenderer_Render.Invoke(null, [logEvent.MessageTemplate, logEvent.Properties, output, format, formatProvider]);
#endif
    }

    /// <summary>Returns the message rendered with Serilog's message-token formatting rules.</summary>
    /// <param name="logEvent">The event supplying the template and properties.</param>
    /// <param name="format">Serilog message-token formatting, or null for defaults.</param>
    /// <param name="formatProvider">Formats property values, or null to use Serilog's default provider.</param>
    /// <returns>The rendered message without an added newline.</returns>
    public static string RenderMessage(this LogEvent logEvent, string? format = null, IFormatProvider? formatProvider = null)
    {
        using var disposable = StringBuilder.GetCached();
        var sw = new StringWriter(disposable.Value);
        logEvent.RenderMessage(sw, format, formatProvider);
        return sw.ToString();
    }

    /// <summary>Replaces the exception on an existing event using its backing field.</summary>
    /// <param name="logEvent">The event to update in place.</param>
    /// <param name="ex">The replacement exception, or null to remove it.</param>
    /// <returns>The same event.</returns>
    public static LogEvent SetException(this LogEvent logEvent, Exception? ex)
    {
#if NET8_0_OR_GREATER
        ref var exceptionRef = ref UnsafeAccessors.LogEvent_Exception(logEvent);
        exceptionRef = ex;
#else
        FieldInfos.LogEvent_Exception.SetValue(logEvent, ex);
#endif
        return logEvent;
    }

    /// <summary>Replaces the level on an existing event without rerunning any logging filters.</summary>
    /// <param name="logEvent">The event to update in place.</param>
    /// <param name="level">The replacement Serilog level.</param>
    /// <returns>The same event.</returns>
    public static LogEvent SetLevel(this LogEvent logEvent, LogEventLevel level)
    {

#if NET8_0_OR_GREATER
        ref var levelRef = ref UnsafeAccessors.LogEvent_Level(logEvent);
        levelRef = level;
#else
        FieldInfos.LogEvent_Level.SetValue(logEvent, level);
#endif
        FieldInfos.LogEvent_Level.SetValue(logEvent, level);
        return logEvent;
    }

    /// <summary>Replaces an event's template while retaining its existing structured properties.</summary>
    /// <param name="logEvent">The event to update in place.</param>
    /// <param name="template">The replacement template, including the tokens used for rendering.</param>
    /// <returns>The same event.</returns>
    public static LogEvent SetMessageTemplate(this LogEvent logEvent, MessageTemplate template)
    {
#if NET8_0_OR_GREATER
        ref var messageTemplateRef = ref UnsafeAccessors.LogEvent_MessageTemplate(logEvent);
        messageTemplateRef = template;
#else
        FieldInfos.LogEvent_MessageTemplate.SetValue(logEvent, template);
#endif
        return logEvent;
    }

    /// <summary>Checks a top-level property's scalar value using object equality.</summary>
    /// <param name="logEvent">The event to inspect.</param>
    /// <param name="propertyName">The exact, case-sensitive top-level name.</param>
    /// <param name="scalarValue">The expected scalar value, including null.</param>
    /// <returns>True only when the named property is scalar and its value equals the expected value.</returns>
    public static bool MatchScalar(this LogEvent logEvent, string propertyName, object? scalarValue)
    {
        return logEvent.Properties.TryGetValue(propertyName, out var value)
               && value is ScalarValue scalar
               && Equals(scalar.Value, scalarValue);
    }

    /// <summary>Checks scalar members of any top-level StructureValue on an event.</summary>
    /// <param name="logEvent">The event to inspect.</param>
    /// <param name="propertyName">The exact member name inside a top-level structure.</param>
    /// <param name="scalarValue">The expected scalar member value, including null.</param>
    /// <returns>True when any such structure contains a matching scalar member. Nested structures are not searched recursively.</returns>
    public static bool MatchStructure(this LogEvent logEvent, string propertyName, object? scalarValue)
    {
        return logEvent.Properties.Any(m => m.Value is StructureValue structureValue
                                            && structureValue.Match(propertyName, scalarValue));
    }

    /// <summary>Checks a structure's immediate scalar members using object equality.</summary>
    /// <param name="structureValue">The structure to inspect.</param>
    /// <param name="propertyName">The exact, case-sensitive member name.</param>
    /// <param name="scalarValue">The expected scalar value, including null.</param>
    /// <returns>True when an immediate scalar member matches; nested values are not searched.</returns>
    public static bool Match(this StructureValue structureValue, string propertyName, object? scalarValue)
    {
        return structureValue.Properties.Any(m => m.Name == propertyName
                                                  && m.Value is ScalarValue scalar
                                                  && Equals(scalar.Value, scalarValue));
    }

    /// <summary>Creates a property and adds it only if the event does not already contain that name.</summary>
    /// <param name="logEvent">The event to enrich in place.</param>
    /// <param name="factory">Creates the structured property even when an existing property will be retained.</param>
    /// <param name="name">The nonempty property name accepted by Serilog.</param>
    /// <param name="value">The property value, including null.</param>
    /// <param name="destructureObjects">Whether the factory should capture object structure.</param>
    public static void TryAddProperty(this LogEvent logEvent, ILogEventPropertyFactory factory,
        string name, object? value, bool destructureObjects = false)
    {
        logEvent.AddPropertyIfAbsent(factory.CreateProperty(name, value, destructureObjects));
    }

    /// <summary>Applies Serilog's hierarchical source matching to an event.</summary>
    /// <param name="logEvent">The event supplying SourceContext.</param>
    /// <param name="source">A case-sensitive source name matching itself or dot-separated descendants.</param>
    /// <returns>True when SourceContext belongs to the specified source.</returns>
    public static bool MatchSource(this LogEvent logEvent, string source)
    {
        return Matching.FromSource(source)(logEvent);
    }

    /// <summary>Evaluates exclusion rules until the first match.</summary>
    /// <param name="e">The event supplied to each evaluated rule.</param>
    /// <param name="items">Rules evaluated in enumeration order.</param>
    /// <returns>True if any rule excludes the event; false for an empty sequence.</returns>
    /// <remarks>Rule and enumeration exceptions propagate to the caller.</remarks>
    public static bool ShouldExclude(this LogEvent e, IEnumerable<ILogEventExcluder> items)
    {
        return items.Any(x => x.ShouldExclude(e));
    }

    /// <summary>Accepts any event when the source restriction is null, otherwise applies hierarchical source matching.</summary>
    /// <param name="e">The event supplying SourceContext.</param>
    /// <param name="source">The required source, or null to disable this restriction.</param>
    /// <returns>True when the restriction is absent or the event matches it.</returns>
    public static bool MatchSourceOrNull(this LogEvent e, string? source)
    {
        return source is null || Matching.FromSource(source)(e);
    }

    /// <summary>Checks whether an event satisfies an optional inclusive maximum level.</summary>
    /// <param name="logEvent">The event whose level is checked.</param>
    /// <param name="maxLevel">The inclusive maximum, or null to accept any event level.</param>
    /// <returns>True when the maximum is absent or the event level is at most the maximum.</returns>
    public static bool MatchMaxLevelOrNull(this LogEvent logEvent, LogEventLevel? maxLevel)
    {
        return maxLevel is null || logEvent.Level <= maxLevel;
    }

    /// <summary>Formats an event into a string with a caller-supplied text formatter.</summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="formatter">Controls the complete output, including any trailing newline.</param>
    /// <returns>The formatter's output without trimming or other transformation.</returns>
    public static string ToString(this LogEvent logEvent, ITextFormatter formatter)
    {
        using var disposable = StringBuilder.GetCached();
        var sw = new StringWriter(disposable.Value);
        formatter.Format(logEvent, sw);
        var str = sw.ToString();
        return str;
    }

    /// <summary>Wraps an event's exception in FormattedException for consistent textual rendering.</summary>
    /// <param name="logEvent">The event to update in place.</param>
    /// <returns>The same event; events with no exception or an existing FormattedException remain unchanged.</returns>
    /// <remarks>The original exception remains accessible through GetOriginalException.</remarks>
    public static LogEvent FormatException(this LogEvent logEvent)
    {
        if (logEvent.Exception is null or FormattedException)
            return logEvent;

        var ex = new FormattedException(logEvent.Exception);
        logEvent.SetException(ex);
        return logEvent;
    }

    /// <summary>Gets the event's original exception through one FormattedException wrapper.</summary>
    /// <param name="logEvent">The event to inspect.</param>
    /// <returns>The wrapped original, the unwrapped exception as-is, or null when absent.</returns>
    public static Exception? GetOriginalException(this LogEvent logEvent)
    {
        if (logEvent.Exception is not { } ex)
            return null;

        if (ex is FormattedException formattedException)
            return formattedException.Exception;

        return ex;
    }

    /// <summary>Unwraps a stack-trace-free exception when its message already appears in the rendered log message or its inner exception's message.</summary>
    /// <param name="logEvent">The event to update in place.</param>
    /// <returns>The same event, with a duplicate outer exception replaced by its inner exception or cleared when there is no inner exception.</returns>
    /// <remarks>
    /// An empty message template is populated with the exception message as literal text before checking
    /// for duplication. Exceptions with a stack trace are retained. Each call considers the outermost
    /// exception and uses case-sensitive substring comparisons for duplicate messages.
    /// </remarks>
    public static LogEvent UnwrapException(this LogEvent logEvent)
    {
        if (logEvent.Exception is not { } ex)
            return logEvent;

        if (ex.StackTrace.IsNotEmpty())
            return logEvent;

        // If the stack trace is empty, we need to unwrap the exception.
        // NOTE: TextToken is required for the message template to be rendered.
        if (logEvent.MessageTemplate.Text.IsNullOrEmpty() && ex.Message is { Length: > 0 } message)
            logEvent.SetMessageTemplate(new MessageTemplate(message, [new TextToken(message)]));

        // if the message contains the exception message, we need to unwrap the exception.
        var inner = ex.InnerException;
        var template = logEvent.MessageTemplate;
        var error = ex.Message;
        if (inner?.Message.Contains(error) == true
            || template.Render(logEvent.Properties).Contains(error))
        {
            logEvent.SetException(inner);
        }

        return logEvent;
    }

    /// <summary>Applies an outer LogException's level and attempts to unwrap its duplicate message.</summary>
    /// <param name="logEvent">The event to update in place.</param>
    /// <returns>The same event; other exception types are unchanged.</returns>
    /// <remarks>This method does not suppress LogLevel.None or recheck minimum levels. SerilogConfiguration performs those checks earlier.</remarks>
    public static LogEvent HandleLogException(this LogEvent logEvent)
    {
        if (logEvent.Exception is not LogException logException)
            return logEvent;

        var level = logException.Level.ToSerilogLevel();
        if (level != logEvent.Level)
        {
            logEvent.SetLevel(level);
        }

        logEvent.UnwrapException();

        return logEvent;
    }

    /// <summary>Attempts duplicate-message unwrapping only when the current exception is a SimpleException.</summary>
    /// <param name="logEvent">The event to update in place.</param>
    /// <returns>The same event; other exception types are unchanged.</returns>
    public static LogEvent HandleSimpleException(this LogEvent logEvent)
    {
        if (logEvent.Exception is SimpleException)
        {
            logEvent.UnwrapException();
        }

        return logEvent;
    }

    /// <summary>Reads a top-level scalar property without converting its value.</summary>
    /// <typeparam name="T">The required runtime value type.</typeparam>
    /// <param name="logEvent">The event to inspect.</param>
    /// <param name="propertyName">The exact, case-sensitive property name.</param>
    /// <returns>The scalar value when assignable to T; otherwise default(T), including for missing, null, or structured values.</returns>
    public static T? GetPropertyValue<T>(this LogEvent logEvent, string propertyName)
    {
        if (logEvent.Properties.TryGetValue(propertyName, out var value) == false)
            return default;

        if (value is ScalarValue scalarValue)
            return scalarValue.Value is T t ? t : default;

        return default;
    }

    /// <summary>Reads the string-valued SourceContext property.</summary>
    /// <param name="logEvent">The event to inspect.</param>
    /// <returns>The category/source string, or null when absent or not a string scalar.</returns>
    public static string? GetSourceContext(this LogEvent logEvent)
    {
        return logEvent.GetPropertyValue<string>(Constants.SourceContext);
    }
}
