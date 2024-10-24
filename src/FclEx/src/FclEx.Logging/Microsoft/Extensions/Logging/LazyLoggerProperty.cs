namespace Microsoft.Extensions.Logging;

public readonly record struct LazyLoggerProperty(string Key, Func<object?> Value)
{
    public static implicit operator LazyLoggerProperty(KeyValuePair<string, Func<object?>> pair)
    {
        return new(pair.Key, pair.Value);
    }

    public static implicit operator LazyLoggerProperty((string Key, Func<object?> Value) pair)
    {
        return new(pair.Key, pair.Value);
    }
}