namespace FclEx.Serilog;

public record SourceFilterItem(string Source, LogEventLevel? MaxLevel = null) : ILogEventFilterItem
{
    public string Source { get; } = Check.NotNull(Source);

    public static implicit operator SourceFilterItem((string Source, LogEventLevel? MaxLevel) tuple)
    {
        return new(tuple.Source, tuple.MaxLevel);
    }

    public bool Match(LogEvent e)
    {
        return e.MatchMaxLeveOrNull(MaxLevel)
               && Matching.FromSource(Source)(e);
    }

    public static readonly SourceFilterItem[] CommonItems =
    [
        ("Microsoft.AspNetCore", LogEventLevel.Information),
        ("System.Net.Http", LogEventLevel.Information),
        ("Microsoft.EntityFrameworkCore", LogEventLevel.Information),
        ("DotNetCore.CAP", LogEventLevel.Information)
    ];
}