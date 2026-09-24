using System;
using Microsoft.CodeAnalysis.Diagnostics;
using System.IO;

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

        var index = path.IndexOf("src", StringComparison.Ordinal);
        if (index < 0)
        {
            throw new InvalidOperationException($"Cannot locate src directory from current path: {path}");
        }

        var assembly = typeof(AnalyzerConfigOptionsProviderExtensions).Assembly.GetName().Name;
        var projectDir = Path.Combine(path[..index], "src", assembly);

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
        return Directory.Exists(projectDir)
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            ? projectDir 
            : throw new InvalidOperationException($"Source generator project directory does not exist: {projectDir}");

    }
}