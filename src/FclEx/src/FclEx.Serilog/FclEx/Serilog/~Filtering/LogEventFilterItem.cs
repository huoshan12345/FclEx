namespace FclEx.Serilog;

public record LogEventFilterItem(Func<LogEvent, bool> Predicate) : ILogEventFilterItem
{
    public bool Match(LogEvent e)
    {
        return Predicate(e);
    }
}