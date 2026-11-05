namespace FclEx.Serilog;

/// <summary>Excludes events whose current exception satisfies a predicate within an optional source restriction.</summary>
/// <param name="Source">The required source or dot-separated ancestor, or null to accept any source.</param>
/// <param name="Predicate">Evaluates the current outer exception; inner or formatted exceptions are not automatically unwrapped.</param>
public record ExceptionExcluder(string? Source, Func<Exception, bool> Predicate) : ILogEventExcluder
{
    /// <summary>Creates an exception exclusion rule from its source restriction and predicate.</summary>
    /// <param name="tuple">The optional source and exception predicate.</param>
    /// <returns>A rule with those settings.</returns>
    public static implicit operator ExceptionExcluder((string? Source, Func<Exception, bool> Predicate) tuple)
    {
        return new(tuple.Source, tuple.Predicate);
    }

    /// <summary>Checks the source, then invokes the predicate only when the event has an exception.</summary>
    /// <param name="e">The event to inspect.</param>
    /// <returns>True when both restrictions match; false for events without an exception.</returns>
    public bool ShouldExclude(LogEvent e)
    {
        return e.MatchSourceOrNull(Source)
               && e.Exception is { } ex
               && Predicate(ex);
    }

    /// <summary>The built-in exception exclusions; currently empty.</summary>
    public static readonly ExceptionExcluder[] CommonItems = [];
}
