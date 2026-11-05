namespace FclEx.Serilog;

/// <summary>Excludes a source and its dot-separated descendants up to an optional inclusive maximum level.</summary>
/// <param name="Source">The case-sensitive SourceContext name or ancestor to match.</param>
/// <param name="MaxLevel">The inclusive maximum, or null to match every event level.</param>
public record SourceExcluder(string Source, LogEventLevel? MaxLevel = null) : ILogEventExcluder
{
    /// <summary>The source name passed to Serilog's hierarchical source matcher.</summary>
    public string Source { get; } = Check.NotNull(Source);

    /// <summary>Creates a source exclusion rule from a source and optional inclusive maximum.</summary>
    /// <param name="tuple">The source and level restriction.</param>
    /// <returns>A rule with those settings.</returns>
    public static implicit operator SourceExcluder((string Source, LogEventLevel? MaxLevel) tuple)
    {
        return new(tuple.Source, tuple.MaxLevel);
    }

    /// <inheritdoc />
    public bool ShouldExclude(LogEvent e)
    {
        return e.MatchMaxLevelOrNull(MaxLevel)
               && Matching.FromSource(Source)(e);
    }

    /// <summary>Built-in ASP.NET Core, HTTP, EF Core, and CAP source exclusions through Information level.</summary>
    public static readonly SourceExcluder[] CommonItems =
    [
        ("Microsoft.AspNetCore", LogEventLevel.Information),
        ("System.Net.Http", LogEventLevel.Information),
        ("Microsoft.EntityFrameworkCore", LogEventLevel.Information),
        ("DotNetCore.CAP", LogEventLevel.Information),
    ];
}
