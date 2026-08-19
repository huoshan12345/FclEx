using FclEx.Logging;
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

    public static bool MatchMaxLeveOrNull(this LogEvent e, LogEventLevel? maxLevel)
    {
        return maxLevel is null || e.Level <= maxLevel;
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
        if (template.Text.Contains(error)
            || inner?.Message.Contains(error) == true
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