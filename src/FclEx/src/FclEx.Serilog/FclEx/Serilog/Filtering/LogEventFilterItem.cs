namespace FclEx.Serilog.Filtering;

public record LogEventFilterItem(Func<LogEvent, bool> Predicate) : ILogEventFilterItem
{
    public bool Match(LogEvent e)
    {
        return Predicate(e);
    }
}