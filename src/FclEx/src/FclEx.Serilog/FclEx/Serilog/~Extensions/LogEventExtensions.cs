namespace FclEx.Serilog;

public static class LogEventExtensions
{
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

    public static bool Match(this LogEvent e, IEnumerable<ILogEventFilterItem> items)
    {
        return items.Any(x => x.Match(e));
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
        using var disposable = ObjectPoolHelper.StringBuilderPool.GetAsDisposable();
        var sw = new StringWriter(disposable.Value);
        formatter.Format(logEvent, sw);
        var str = sw.ToString();
        return str;
    }
}