namespace FclEx.Serilog;

public static class LogContextPropertiesExtensions
{
    public static LogContextProperties Push<T>(this LogContextProperties properties, IEnumerable<KeyValuePair<string, T>> pairs, bool destructureObjects = false)
    {
        foreach (var (key, value) in pairs.EmptyIfNull())
        {
            properties.Push(key, value, destructureObjects);
        }
        return properties;
    }
}