using Microsoft.CodeAnalysis.Diagnostics;

namespace FclEx.CodeAnalysis;

public static class AnalyzerConfigOptionsProviderExtensions
{
    public static string? GetGlobalOption(this AnalyzerConfigOptionsProvider provider, string key)
    {
        return provider.GlobalOptions.TryGetValue(key, out var info) ? info : null;
    }
}