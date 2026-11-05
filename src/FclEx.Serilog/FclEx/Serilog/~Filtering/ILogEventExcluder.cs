namespace FclEx.Serilog;

/// <summary>Decides whether a structured log event should be suppressed by an exclusion filter.</summary>
public interface ILogEventExcluder
{
    /// <summary>Evaluates one event without changing its logging configuration.</summary>
    /// <param name="e">The event to inspect.</param>
    /// <returns>True to suppress the event; false to keep evaluating the pipeline.</returns>
    bool ShouldExclude(LogEvent e);
}
