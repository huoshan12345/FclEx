namespace FclEx.Serilog;

/// <summary>Adds Serilog object destructuring conventions to Microsoft property scope groups.</summary>
public static class LoggerPropertyScopeExtensions
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

    /// <summary>Pushes a property, optionally prefixing its name with @ for Serilog object destructuring.</summary>
    /// <param name="properties">The scope group that owns the new property scope.</param>
    /// <param name="name">The property name; an existing @ prefix is preserved.</param>
    /// <param name="value">The value retained by the scope.</param>
    /// <param name="destructureObjects">Whether to use Serilog's object destructuring convention.</param>
    /// <returns>The same scope group for further pushes.</returns>
    /// <remarks>Object destructuring requires a Serilog provider; other providers may retain the @ prefix literally.</remarks>
    public static LoggerPropertyScope Push(this LoggerPropertyScope properties, string name, object? value, bool destructureObjects)
    {
        return properties.Push(GetName(name, destructureObjects), value);
    }
}
