namespace FclEx.Serilog.Filtering;

public record PropertyFilterItem(string? Source, string Name, string? Value, LogEventLevel? MaxLevel = null) : ILogEventFilterItem
{
    public string Name { get; } = Check.NotNull(Name);

    public static implicit operator PropertyFilterItem((string? Source, string PropertyName, string? PropertyValue, LogEventLevel? MaxLevel) tuple)
    {
        return new(tuple.Source, tuple.PropertyName, tuple.PropertyValue, tuple.MaxLevel);
    }

    public static implicit operator PropertyFilterItem((string? Source, string PropertyName, string? PropertyValue) tuple)
    {
        return new(tuple.Source, tuple.PropertyName, tuple.PropertyValue);
    }

    public bool Match(LogEvent e)
    {
        if (e.MatchMaxLeveOrNull(MaxLevel) == false)
            return false;

        if (e.MatchSourceOrNull(Source) == false)
            return false;

        return Value == null
            ? Matching.WithProperty(Name)(e)
            : Matching.WithProperty<string>(Name, y => y.Contains(Value))(e);
    }

    public static readonly PropertyFilterItem[] CommonItems = [];

}