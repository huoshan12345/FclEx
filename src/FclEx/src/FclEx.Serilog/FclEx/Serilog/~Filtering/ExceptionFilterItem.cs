namespace FclEx.Serilog;

public record ExceptionFilterItem(string? Source, Func<Exception, bool> Predicate) : ILogEventFilterItem
{
    public static implicit operator ExceptionFilterItem((string? Source, Func<Exception, bool> Predicate) tuple)
    {
        return new(tuple.Source, tuple.Predicate);
    }

    public bool Match(LogEvent e)
    {
        return e.MatchSourceOrNull(Source)
               && e.Exception is { } ex
               && Predicate(ex);
    }

    public static readonly ExceptionFilterItem[] CommonItems = [];
}