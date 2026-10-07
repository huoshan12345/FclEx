namespace FclEx.Logging;

/// <summary>A structured property definition used by a logger wrapper or property scope.</summary>
/// <param name="Key">The property name.</param>
/// <param name="Value">The property value, retained by reference rather than deep copied.</param>
public readonly record struct LoggerProperty(string Key, object? Value)
{
    /// <summary>Creates a property definition from a name and value pair.</summary>
    /// <param name="pair">The property name and value.</param>
    /// <returns>The corresponding property definition.</returns>
    public static implicit operator LoggerProperty(KeyValuePair<string, object?> pair)
    {
        return new(pair.Key, pair.Value);
    }

    /// <summary>Creates a property definition from a name and value tuple.</summary>
    /// <param name="pair">The property name and value.</param>
    /// <returns>The corresponding property definition.</returns>
    public static implicit operator LoggerProperty((string Key, object? Value) pair)
    {
        return new(pair.Key, pair.Value);
    }
}
