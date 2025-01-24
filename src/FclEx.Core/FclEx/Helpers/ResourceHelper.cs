namespace FclEx.Helpers;

public static class ResourceHelper
{
    private static readonly char[] _newLineChars = Environment.NewLine.ToCharArray();

    public static Stream? LoadEmbeddedResource(Assembly assembly, string name)
    {
        var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(name));
        var stream = resourceName == null ? null : assembly.GetManifestResourceStream(resourceName);
        return stream;
    }

    public static T LoadEmbeddedResource<T>(Assembly assembly, string name, Func<Stream, T> func)
    {
        using var resource = LoadEmbeddedResource(assembly, name) ?? throw new KeyNotFoundException($"Cannot find embedded resource by name '{name}'");
        return func(resource);
    }

    public static string LoadStringFromEmbeddedResource(Assembly assembly, string resourceName, Encoding encoding) => LoadEmbeddedResource(assembly, resourceName, s =>
    {
        using var sr = new StreamReader(s, encoding);
        return sr.ReadToEnd();
    });

    public static string LoadStringFromEmbeddedResource(Assembly assembly, string resourceName) =>
        LoadStringFromEmbeddedResource(assembly, resourceName, Encoding.UTF8);

    public static string[] LoadLinesFromEmbeddedResource(Assembly assembly, string resourceName, Encoding encoding)
    {
        return LoadStringFromEmbeddedResource(assembly, resourceName, encoding)
            .Split(_newLineChars, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] LoadLinesFromEmbeddedResource(Assembly assembly, string resourceName) =>
        LoadLinesFromEmbeddedResource(assembly, resourceName, Encoding.UTF8);
}