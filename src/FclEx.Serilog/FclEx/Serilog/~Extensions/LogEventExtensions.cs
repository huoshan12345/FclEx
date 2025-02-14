using static FclEx.Serilog.Fields;

namespace FclEx.Serilog;

public static class LogEventExtensions
{
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
        using var disposable = StringBuilderHelper.GetCached();
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

    public static LogEvent UnwrapException(this LogEvent logEvent)
    {
        if (logEvent.Exception is not { } ex)
            return logEvent;

        if (ex.StackTrace.IsNotEmpty())
            return logEvent;

        // If the stack trace is empty, we need to unwrap the exception.

        if (logEvent.MessageTemplate.Text.IsNullOrEmpty() && ex.Message is { Length: > 0 } message)
            logEvent.SetMessageTemplate(new MessageTemplate(message, []));

        var inner = ex.InnerException;
        logEvent.SetException(inner);

        return logEvent;
    }

    public static LogEvent HandleLogException(this LogEvent logEvent)
    {
        if (logEvent.Exception is not LogException logException)
            return logEvent;

        if (logException.Level != logEvent.Level)
        {
            logEvent.SetLevel(logException.Level);
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
}