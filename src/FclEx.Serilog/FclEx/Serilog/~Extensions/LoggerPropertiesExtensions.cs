namespace FclEx.Serilog;

public static class LoggerPropertiesExtensions
{
    private static readonly ConcurrentDictionary<string, string> _names = new();

    private static string GetName(string name, bool destructureObjects)
    {
        if (destructureObjects == false)
            return name;

        return name is [var ch, ..] && ch != '@'
            ? _names.GetOrAdd(name, m => '@' + m)
            : name;
    }

    public static LoggerProperties Push(this LoggerProperties properties, string name, object? value, bool destructureObjects)
    {
        return properties.Push(GetName(name, destructureObjects), value);
    }
}
