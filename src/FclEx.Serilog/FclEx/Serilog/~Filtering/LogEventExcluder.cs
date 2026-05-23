namespace FclEx.Serilog;

public record LogEventExcluder(Func<LogEvent, bool> Predicate) : ILogEventExcluder
{
    public bool ShouldExclude(LogEvent e)
    {
        return Predicate(e);
    }
}