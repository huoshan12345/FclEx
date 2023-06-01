using System.IO;
using System.Linq;

namespace FclEx.Helpers;

public static class ResourceHelper
{
    private static readonly char[] _newLineChars = Environment.NewLine.ToCharArray();

    public static Stream? LoadEmbededResource(Assembly assembly, string name)
    {
        var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith(name));
        var stream = resourceName == null ? null : assembly.GetManifestResourceStream(resourceName);
        return stream;
    }

    public static T LoadEmbededResource<T>(Assembly assembly, string name, Func<Stream?, T> func)
    {
        using var resource = LoadEmbededResource(assembly, name);
        return func(resource);
    }

    public static string LoadStringFromEmbededResource(Assembly assembly, string resourceName, Encoding encoding) => LoadEmbededResource(assembly, resourceName, s =>
    {
        using var sr = new StreamReader(s!, encoding);
        return sr.ReadToEnd();
    });

    public static string LoadStringFromEmbededResource(Assembly assembly, string resourceName) =>
        LoadStringFromEmbededResource(assembly, resourceName, Encoding.UTF8);

    public static string[] LoadLinesFromEmbededResource(Assembly assembly, string resourceName, Encoding encoding)
    {
        return LoadStringFromEmbededResource(assembly, resourceName, encoding)
            .Split(_newLineChars, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] LoadLinesFromEmbededResource(Assembly assembly, string resourceName) =>
        LoadLinesFromEmbededResource(assembly, resourceName, Encoding.UTF8);
}