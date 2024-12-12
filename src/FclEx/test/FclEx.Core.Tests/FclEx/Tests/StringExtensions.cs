using Microsoft.CodeAnalysis.CSharp;

namespace FclEx.Tests;

public static class StringExtensions
{
    public static string ToLiteral(this string value)
    {
        return SymbolDisplay.FormatLiteral(value, false);
    }

    public static string ToLiteral(this char value)
    {
        return value.ToString().ToLiteral();
    }

    private static readonly string[] KnownAssemblyPrefixes =
    [
        "System",
        "xunit",
    ];

    private static string? GetCurrentTestingAssembly()
    {
        var frames = new StackTrace().GetFrames();

        for (var i = frames.Length - 1; i >= 0; i--)
        {
            var frame = frames[i];

            var assembly = frame.GetMethod()?.ReflectedType?.Assembly;
            if (assembly?.GetName().Name is not { Length: > 0 } name)
                continue;

            if (KnownAssemblyPrefixes.Any(m => name.StartsWith(m)))
                continue;

            return name;
        }

        return null;
    }

    public static string WithAssemblyInfo(this string str, char separator = '_')
    {
        // used to ensure every test assembly uses unique service, such as database.
        return StringBuilderHelper.Build(m =>
        {
            if (str.IsNotEmpty())
            {
                m.Append(str);
                m.Append(separator);
            }

            var assemblyName = GetCurrentTestingAssembly();
            if (assemblyName.IsNotEmpty())
            {
                // as short as possible cause database don't like long name.
                var name = assemblyName.TrimStart("FclEx").TrimEnd("Tests").Replace(".", "").ToLower();
                if (name.IsNotEmpty())
                {
                    m.Append(name);
                    m.Append(separator);
                }
            }

            m.Append(Environment.Version.Major);
        });
    }
}