namespace FclEx.Serilog;

public static class LogContextPropertiesExtensions
{
    public static LogContextProperties Push<T>(this LogContextProperties properties, IEnumerable<KeyValuePair<string, T>> pairs)
    {
        foreach (var (key, value) in pairs.EmptyIfNull())
        {
            properties.Push(key, value);
        }
        return properties;
    }
}