namespace FclEx.Serilog;

public record SourceExcluder(string Source, LogEventLevel? MaxLevel = null) : ILogEventExcluder
{
    public string Source { get; } = Check.NotNull(Source);

    public static implicit operator SourceExcluder((string Source, LogEventLevel? MaxLevel) tuple)
    {
        return new(tuple.Source, tuple.MaxLevel);
    }

    public bool ShouldExclude(LogEvent e)
    {
        return e.MatchMaxLeveOrNull(MaxLevel)
               && Matching.FromSource(Source)(e);
    }

    public static readonly SourceExcluder[] CommonItems =
    [
        ("Microsoft.AspNetCore", LogEventLevel.Information),
        ("System.Net.Http", LogEventLevel.Information),
        ("Microsoft.EntityFrameworkCore", LogEventLevel.Information),
        ("DotNetCore.CAP", LogEventLevel.Information),
    ];
}