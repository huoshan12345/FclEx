namespace FclEx.Serilog;

/// <summary>Adapts a caller-supplied event predicate to an exclusion rule.</summary>
/// <param name="Predicate">Returns true to exclude an event. Exceptions from the predicate propagate when this rule is called directly.</param>
public record LogEventExcluder(Func<LogEvent, bool> Predicate) : ILogEventExcluder
{
    /// <inheritdoc />
    public bool ShouldExclude(LogEvent e)
    {
        return Predicate(e);
    }
}
