using System.Text.RegularExpressions;

namespace FclEx.Sources.Abp;

public class AbpCoreUsingsSource
{
    public const string Alias = "AbpCore";

    public static readonly Regex[] ExcludeNamespacePatterns =
    [
        new(@"^System(\.|$)"),
        new(@"^Volo\.Abp\.Threading$"),
    ];

    public static SourceInfo Generate(IAssemblySymbol abpCoreAssembly)
    {
        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine();

        builder.WriteLine($"extern alias {Alias};");
        builder.WriteLine();

        var namespaces = new SortedSet<string>();

        var queue = new Queue<INamespaceSymbol>([abpCoreAssembly.GlobalNamespace]);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var childNs in current.GetNamespaceMembers())
            {
                queue.Enqueue(childNs);
            }

            if (current.GetTypeMembers().Any(m => m.DeclaredAccessibility == Accessibility.Public) == false)
                continue;

            namespaces.Add(current.ToDisplayString());
        }

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var ns in namespaces)
        {
            if (ExcludeNamespacePatterns.Any(p => p.IsMatch(ns)))
                continue;

            builder.WriteLine($"global using {Alias}::{ns};");
        }

        builder.WriteLine();
        builder.WriteLine("global using Check = FclEx.Check;");
        builder.WriteLine("global using FileHelper = FclEx.Helpers.FileHelper;");
        builder.WriteLine("global using ObjectHelper = FclEx.Helpers.ObjectHelper;");

        return new SourceInfo(true, "AbpCoreUsings.cs", builder.ToString());
    }
}
