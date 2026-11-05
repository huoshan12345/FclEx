using Serilog.Parsing;
using static FclEx.Serilog.Fields;

namespace FclEx.Serilog;

public static class LogEventExtensions
{
    public static void RenderMessage(this LogEvent logEvent, TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
    {
        Methods.MessageTemplateRenderer_Render.Invoke(null, [logEvent.MessageTemplate, logEvent.Properties, output, format, formatProvider]);
    }

    public static string RenderMessage(this LogEvent logEvent, string? format = null, IFormatProvider? formatProvider = null)
    {
        using var disposable = StringBuilder.GetCached();
        var sw = new StringWriter(disposable.Value);
        RenderMessage(logEvent, sw, format, formatProvider);
        return sw.ToString();
    }

    public static LogEvent SetException(this LogEvent logEvent, Exception? ex)
    {
        LogEvent_Exception.SetValue(logEvent, ex);
        return logEvent;
    }

    public static LogEvent SetLevel(this LogEvent logEvent, LogEventLevel level)
    {
        LogEvent_Level.SetValue(logEvent, level);
        return logEvent;
    }

    public static LogEvent SetMessageTemplate(this LogEvent logEvent, MessageTemplate template)
    {
        LogEvent_MessageTemplate.SetValue(logEvent, template);
        return logEvent;
    }

    public static bool MatchScalar(this LogEvent logEvent, string propertyName, object scalarValue)
    {
        return logEvent.Properties.TryGetValue(propertyName, out var value)
               && value is ScalarValue scalar
               && Equals(scalar.Value, scalarValue);
    }

    public static bool MatchStructure(this LogEvent logEvent, string propertyName, object scalarValue)
    {
        return logEvent.Properties.Any(m => m.Value is StructureValue structureValue
                                            && Match(structureValue, propertyName, scalarValue));
    }

    public static bool Match(this StructureValue structureValue, string propertyName, object scalarValue)
    {
        return structureValue.Properties.Any(m => m.Name == propertyName
                                                  && m.Value is ScalarValue scalar
                                                  && Equals(scalar.Value, scalarValue));
    }

    public static void TryAddProperty(this LogEvent logEvent, ILogEventPropertyFactory factory,
        string name, object? value, bool destructureObjects = false)
    {
        logEvent.AddPropertyIfAbsent(factory.CreateProperty(name, value, destructureObjects));
    }

    public static bool MatchSource(this LogEvent logEvent, string source)
    {
        return Matching.FromSource(source)(logEvent);
    }

    public static bool ShouldExclude(this LogEvent e, IEnumerable<ILogEventExcluder> items)
    {
        return items.Any(x => x.ShouldExclude(e));
    }

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

    public static string ToString(this LogEvent logEvent, ITextFormatter formatter)
    {
        using var disposable = StringBuilder.GetCached();
        var sw = new StringWriter(disposable.Value);
        formatter.Format(logEvent, sw);
        var str = sw.ToString();
        return str;
    }

    public static LogEvent FormatException(this LogEvent logEvent)
    {
        if (logEvent.Exception is null or FormattedException)
            return logEvent;

        var ex = new FormattedException(logEvent.Exception);
        logEvent.SetException(ex);
        return logEvent;
    }

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

    public static LogEvent HandleSimpleException(this LogEvent logEvent)
    {
        if (logEvent.Exception is SimpleException)
        {
            logEvent.UnwrapException();
        }

        return logEvent;
    }

    public static T? GetPropertyValue<T>(this LogEvent logEvent, string propertyName)
    {
        if (logEvent.Properties.TryGetValue(propertyName, out var value) == false)
            return default;

        if (value is ScalarValue scalarValue)
            return scalarValue.Value is T t ? t : default;

        return default;
    }

    public static string? GetSourceContext(this LogEvent logEvent)
    {
        return logEvent.GetPropertyValue<string>(Constants.SourceContext);
    }
}
