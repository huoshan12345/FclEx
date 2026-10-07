namespace FclEx.Logging;

/// <summary>A property whose value is computed once for each enabled log call made through a <see cref="PropertiesLogger"/>.</summary>
/// <param name="Key">The structured property name.</param>
/// <param name="ValueFactory">The callback producing the value. It must not be null; exceptions propagate from the log call.</param>
public readonly record struct LazyLoggerProperty(string Key, Func<object?> ValueFactory)
{
    /// <summary>Creates a lazy property from a name and callback pair.</summary>
    /// <param name="pair">The property name and value factory.</param>
    /// <returns>The corresponding lazy property definition.</returns>
    public static implicit operator LazyLoggerProperty(KeyValuePair<string, Func<object?>> pair)
    {
        return new(pair.Key, pair.Value);
    }

    /// <summary>Creates a lazy property from a name and callback tuple.</summary>
    /// <param name="pair">The property name and value factory.</param>
    /// <returns>The corresponding lazy property definition.</returns>
    public static implicit operator LazyLoggerProperty((string Key, Func<object?> ValueFactory) pair)
    {
        return new(pair.Key, pair.ValueFactory);
    }
}
