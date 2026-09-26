#pragma warning disable RS1035

using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.IO;
using System.Text.Json;

namespace FclEx.Sources.Core.Tests;

internal class MediaTypeNamesSourceTestsSource
{
    private const string @namespace = "System.Net.Mime";
    private const string typeName = "MediaTypeNames";
    private const string className = $"{typeName}Tests";

    private static readonly string[] _usings =
    [
        "System.Net.Mime",
        "FclEx.Extensions",
    ];

    internal static SourceInfo Generate(SourceProductionContext context, AnalyzerConfigOptionsProvider options)
    {
        var nestedClasses = GetNestedClasses(context, options);
        if (nestedClasses is null)
        {
            return SourceInfo.Failed;
        }

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine()
            .WriteUsings(_usings)
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine($"public class {className}")
            .WriteOpeningBracket();

        foreach (var nestedClass in nestedClasses)
        {
            builder.WriteLine("[Fact]");
            builder.WriteLine($"public void {nestedClass.Name}_Test()");
            builder.WriteOpeningBracket();

            foreach (var field in nestedClass.Fields)
            {
                builder.WriteLine($"Assert.Equal(\"{field.Value}\", {typeName}.{nestedClass.Name}.{field.Name});");
            }

            builder.WriteClosingBracket();
            builder.WriteLine();
        }

        // End class declaration
        builder.WriteClosingBracket();

        // End namespace declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }

    private static IReadOnlyList<NestedClass>? GetNestedClasses(SourceProductionContext context, AnalyzerConfigOptionsProvider options)
    {
        var resourcesDir = GetResourcesDir();
        if (resourcesDir is null)
            return null;

        var file = new FileInfo(Path.Combine(resourcesDir, $"{typeName}.json"));
        if (file.Exists == false)
        {
            Report("File '{0}' does not exist.", file.Name);
            return null;
        }

        var text = File.ReadAllText(file.FullName);
        return JsonSerializer.Deserialize<IReadOnlyList<NestedClass>>(text);

        string? GetResourcesDir()
        {
            try
            {
                var projectDir = options.GetProjectDir();
                return Path.Combine(projectDir, "Resources");
            }
            catch (Exception ex)
            {
                Report(ex.Message);
                return null;
            }
        }

        void Report(string messageFormat, params object?[]? args)
        {
            var descriptor = new DiagnosticDescriptor(
                id: "FclEx",
                title: nameof(GetNestedClasses),
                messageFormat: messageFormat,
                category: nameof(MediaTypeNamesSource),
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);
            context.ReportDiagnostic(Diagnostic.Create(descriptor, null, messageArgs: args));
        }
    }
}
