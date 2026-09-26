#pragma warning disable RS1035
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

        var root = FindSolutionRoot(path);
        if (root is null)
            throw new InvalidOperationException($"Cannot locate solution directory from current path: {path}");

        var assembly = typeof(AnalyzerConfigOptionsProviderExtensions).Assembly.GetName().Name;
        var projectDir = Path.Combine(root.FullName, "src", assembly);

        return Directory.Exists(projectDir)
            ? projectDir
            : throw new InvalidOperationException($"Source generator project directory does not exist: {projectDir}");

        static DirectoryInfo? FindSolutionRoot(string path)
        {
            DirectoryInfo? cur = new(path);
            while (cur != null)
            {
                if (cur.Name is "src" or "test"
                   && cur.Parent is { Name: "FclEx" } parent)
                {
                    return parent;
                }

                cur = cur.Parent;
            }

            return null;
        }

    }
}