#pragma warning disable IDE0005
using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
#pragma warning restore IDE0005

namespace FclEx.CodeAnalysis;

public static class AnalyzerConfigOptionsProviderExtensions
{
    public static string? GetGlobalOption(this AnalyzerConfigOptionsProvider provider, string key)
    {
        return provider.GlobalOptions.TryGetValue(key, out var info) ? info : null;
    }

    public static string GetProjectDir(this AnalyzerConfigOptionsProvider provider)
    {
        const string key = "build_property.projectdir";
        var path = provider.GetGlobalOption(key);
        if (path is null)
        {
            throw new InvalidOperationException($"Cannot find global option by key '{key}'");
        }

        var segments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var index = Array.FindLastIndex(segments, s => string.Equals(s, "FclEx", StringComparison.Ordinal));
        if (index < 0)
            throw new InvalidOperationException($"Cannot locate solution directory from current path: {path}");

        var assembly = typeof(AnalyzerConfigOptionsProviderExtensions).Assembly.GetName().Name;
        var parts = segments.Take(index + 1).ToList();
        parts.Add("src");
        parts.Add(assembly);
        var projectDir = Path.Combine(parts.ToArray());

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
        return Directory.Exists(projectDir)
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            ? projectDir
            : throw new InvalidOperationException($"Source generator project directory does not exist: {projectDir}");

    }
}