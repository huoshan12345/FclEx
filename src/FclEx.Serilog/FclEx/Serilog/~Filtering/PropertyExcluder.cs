namespace FclEx.Serilog;

/// <summary>Excludes events by top-level property presence or a substring of a string scalar property.</summary>
/// <param name="Source">The required source or dot-separated ancestor, or null for any source.</param>
/// <param name="Name">The exact, case-sensitive property name.</param>
/// <param name="Value">A case-sensitive substring of a string scalar, or null to match property presence regardless of value type.</param>
/// <param name="MaxLevel">An inclusive maximum level, or null for no level restriction.</param>
public record PropertyExcluder(string? Source, string Name, string? Value, LogEventLevel? MaxLevel = null) : ILogEventExcluder
{
    /// <summary>The property name used for an exact top-level lookup.</summary>
    public string Name { get; } = Check.NotNull(Name);

    /// <summary>Creates a property rule from its source, name, substring, and maximum level.</summary>
    /// <param name="tuple">The rule settings.</param>
    /// <returns>A rule with those settings.</returns>
    public static implicit operator PropertyExcluder((string? Source, string PropertyName, string? PropertyValue, LogEventLevel? MaxLevel) tuple)
    {
        return new(tuple.Source, tuple.PropertyName, tuple.PropertyValue, tuple.MaxLevel);
    }

    /// <summary>Creates a property rule with no maximum level restriction.</summary>
    /// <param name="tuple">The source, property name, and optional string substring.</param>
    /// <returns>A rule whose MaxLevel is null.</returns>
    public static implicit operator PropertyExcluder((string? Source, string PropertyName, string? PropertyValue) tuple)
    {
        return new(tuple.Source, tuple.PropertyName, tuple.PropertyValue);
    }

    /// <summary>Checks level and source, then property presence or the requested string substring.</summary>
    /// <param name="e">The event to inspect.</param>
    /// <returns>True when all restrictions match; non-string values cannot match a non-null Value restriction.</returns>
    public bool ShouldExclude(LogEvent e)
    {
        if (e.MatchMaxLevelOrNull(MaxLevel) == false)
            return false;

        if (e.MatchSourceOrNull(Source) == false)
            return false;

        return Value == null
            ? Matching.WithProperty(Name)(e)
            : Matching.WithProperty<string>(Name, y => y.Contains(Value))(e);
    }

    /// <summary>The built-in property exclusions; currently empty.</summary>
    public static readonly PropertyExcluder[] CommonItems = [];

}
