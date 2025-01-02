namespace FclEx.Serilog;

public record ExceptionExcluder(string? Source, Func<Exception, bool> Predicate) : ILogEventExcluder
{
    public static implicit operator ExceptionExcluder((string? Source, Func<Exception, bool> Predicate) tuple)
    {
        return new(tuple.Source, tuple.Predicate);
    }

    public bool ShouldExclude(LogEvent e)
    {
        return e.MatchSourceOrNull(Source)
               && e.Exception is { } ex
               && Predicate(ex);
    }

    public static readonly ExceptionExcluder[] CommonItems = [];
}