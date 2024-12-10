namespace FclEx.Logging;

public readonly record struct LoggerProperty(string Key, object? Value)
{
    public static implicit operator LoggerProperty(KeyValuePair<string, object?> pair)
    {
        return new(pair.Key, pair.Value);
    }

    public static implicit operator LoggerProperty((string Key, object? Value) pair)
    {
        return new(pair.Key, pair.Value);
    }
}