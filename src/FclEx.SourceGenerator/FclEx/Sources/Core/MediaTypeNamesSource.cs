#pragma warning disable RS1035

using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FclEx.Sources.Core;

internal class MediaTypeNamesSource
{
    private const string @namespace = "FclEx.Extensions";
    private const string typeName = "MediaTypeNames";

    private static readonly string[] _usings =
    [
        "System.Net.Mime",
    ];

    internal static IEnumerable<SourceInfo> Generate(SourceProductionContext context, AnalyzerConfigOptionsProvider options)
    {
        SynchronizationContext.SetSynchronizationContext(null);
        var nestedClasses = GetNestedClasses(context, options).GetAwaiter().GetResult();
        if (nestedClasses is null)
        {
            yield return SourceInfo.Failed;
            yield break;
        }

        var missingClasses = new Dictionary<string, string>
        {
            ["Font"] = "NET5_0_OR_GREATER",
            ["Multipart"] = "NET5_0_OR_GREATER",
            ["Video"] = "NET11_0_OR_GREATER",
        };

        foreach (var nestedClass in nestedClasses)
        {
            using var builder = new SourceBuilder()
                .WriteGeneratedHeader()
                .WriteLine()
                .WriteUsings(_usings)
                .WriteLine();

            // Namespace declaration
            builder.WriteNamespace(@namespace)
                .WriteOpeningBracket();

            if (missingClasses.TryGetValue(nestedClass.Name, out var condition))
            {
                builder.WriteIf(condition);
                WriteExistingClass(builder, nestedClass);
                builder.WriteElse();
                WriteMissingClass(builder, nestedClass);
                builder.WriteEndIf();
            }
            else
            {
                WriteExistingClass(builder, nestedClass);
            }

            // End namespace declaration
            builder.WriteClosingBracket();

            var str = builder.ToString();
            yield return ($"{typeName}{nestedClass.Name}Extensions.g.cs", str);
        }
    }

    private static void WriteExistingClass(SourceBuilder builder, NestedClass nestedClass)
    {
        // Class declaration
        builder.WriteLine($"public static partial class {typeName}{nestedClass.Name}Extensions")
            .WriteOpeningBracket();

        foreach (var field in nestedClass.Fields)
        {
            builder.WriteLine($"public const string {nestedClass.Name}_{field.Name} = \"{field.Value}\";");
        }
        builder.WriteLine();

        // Extension declaration
        builder.WriteLine($"extension({typeName}.{nestedClass.Name})")
            .WriteOpeningBracket();

        foreach (var field in nestedClass.Fields)
        {
            foreach (var line in field.DocLines)
            {
                builder.WriteLine(line);
            }
            builder.WriteLine($"public static string {field.Name} => {nestedClass.Name}_{field.Name};");
        }

        // End class extension declaration
        builder.WriteClosingBracket();

        // End class declaration
        builder.WriteClosingBracket();
    }

    private static readonly Regex _regSeeCref = new($"""<see cref="(?<type>{typeName}).[^"]+"\s*/>""", RegexOptions.Compiled);

    private static void WriteMissingClass(SourceBuilder builder, NestedClass nestedClass)
    {
        const string className = $"{typeName}Extensions";

        // Class declaration
        builder.WriteLine($"public static partial class {className}")
            .WriteOpeningBracket();

        var name = nestedClass.Name;
        // Class declaration
        builder.WriteLine($"public class {name}")
            .WriteOpeningBracket();

        builder.WriteLine($"public static readonly {name} Instance = new();");

        builder.WriteLine($"private {name}() {{ }}");

        foreach (var field in nestedClass.Fields)
        {
            foreach (var line in field.DocLines)
            {
                var l = _regSeeCref.Replace(line, m => m.Groups["type"].Replace(line, s => className));

                builder.WriteLine(l);
            }
            builder.WriteLine($"public readonly string {field.Name} = \"{field.Value}\";");
        }

        // End class declaration
        builder.WriteClosingBracket();

        builder.WriteLine();

        // Extension declaration
        builder.WriteLine($"extension({typeName})")
            .WriteOpeningBracket();

        foreach (var line in nestedClass.DocLines)
        {
            builder.WriteLine(line);
        }
        builder.WriteLine($"public static {name} {name} => {name}.Instance;");

        // End class extension declaration
        builder.WriteClosingBracket();

        // End class declaration
        builder.WriteClosingBracket();
    }

    private static async Task<IReadOnlyList<NestedClass>?> GetNestedClasses(SourceProductionContext context, AnalyzerConfigOptionsProvider options)
    {
        await Task.Yield();

        var resourcesDir = GetResourcesDir();
        if (resourcesDir is null)
            return null;

        var file = new FileInfo(Path.Combine(resourcesDir, $"{typeName}.json"));
        if (file.Exists)
        {
            // file is updated within 7 days
            // Or it is running under GitHub action
            if (file.LastWriteTimeUtc > DateTime.UtcNow.AddDays(-7) || IsGithubAction || IsDependabot)
            {
                var text = File.ReadAllText(file.FullName);
                return JsonSerializer.Deserialize<IReadOnlyList<NestedClass>>(text);
            }
        }

        var source = await FetchSource();

        if (Directory.Exists(resourcesDir) == false)
            Directory.CreateDirectory(resourcesDir);

        var nestedClasses = MediaTypeNamesParser.Parse(source);
        var json = JsonSerializer.Serialize(nestedClasses, new JsonSerializerOptions
        {
            WriteIndented = true,
        });

        using var writer = new StreamWriter(file.FullName, false);
        await writer.WriteAsync(json);

        return nestedClasses;

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

        static async Task<string> FetchSource()
        {
            await Task.Yield();

            // ReSharper disable once ShortLivedHttpClient
            using var httpClient = new HttpClient();
            const string url = "https://raw.githubusercontent.com/dotnet/dotnet/refs/heads/main/src/runtime/src/libraries/System.Net.Mail/src/System/Net/Mime/MediaTypeNames.cs";
            var text = await httpClient.GetStringAsync(url).ConfigureAwait(false);
            return text;
        }
    }
}

public sealed record ConstField(string Name, string Value, string[] DocLines);
public sealed record NestedClass(string Name, IReadOnlyList<ConstField> Fields, string[] DocLines);

public static class MediaTypeNamesParser
{
    // Step 1: isolate the body of the outer "public static class MediaTypeNames { ... }" wrapper.
    // Anchoring on its own indent + a trailing "}\n}" (closing the class, then the namespace block)
    // stops it from being confused with the nested classes it contains.
    private static readonly Regex OuterClassRegex = new(
        @"public static class MediaTypeNames\s*\r?\n(?<indent>[ \t]*)\{(?<body>.*)\r?\n\k<indent>\}\s*\r?\n\}\s*$",
        RegexOptions.Singleline | RegexOptions.Compiled);

    // Step 2: within that body, match each nested "public static class Xxx { ... }" block.
    // The closing brace must be at the SAME indent as its own "public static class" line —
    // this is what keeps the match from swallowing sibling classes (non-greedy alone isn't enough,
    // since a naive .*? would still stop at the first brace at any indent).
    private static readonly Regex NestedClassRegex = new(
        @"(?<doc>(?:[ \t]*///[^\r\n]*\r?\n)+)[ \t]*public static class (?<name>\w+)\s*\r?\n(?<indent>[ \t]*)\{(?<body>.*?)\r?\n\k<indent>\}",
        RegexOptions.Singleline | RegexOptions.Compiled);

    // Step 3: within a nested class body, match each doc-commented const string field.
    private static readonly Regex FieldRegex = new(
        """(?<doc>(?:[ \t]*///[^\r\n]*\r?\n)+)[ \t]*public const string (?<name>\w+)\s*=\s*"(?<value>[^"]*)";""",
        RegexOptions.Compiled);

    public static IReadOnlyList<NestedClass> Parse(string source)
    {
        var outerMatch = OuterClassRegex.Match(source);
        if (!outerMatch.Success)
            throw new InvalidOperationException("Could not locate 'public static class MediaTypeNames { ... }' wrapper.");

        var outerBody = outerMatch.Groups["body"].Value;
        var result = new List<NestedClass>();

        foreach (Match classMatch in NestedClassRegex.Matches(outerBody))
        {
            var className = classMatch.Groups["name"].Value;
            var classBody = classMatch.Groups["body"].Value;

            var classDoc = classMatch.Groups["doc"].Value;
            var classDocLines = GetDocLines(classDoc);

            var fields = new List<ConstField>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (Match fieldMatch in FieldRegex.Matches(classBody))
            {
                var doc = fieldMatch.Groups["doc"].Value;
                var docLines = GetDocLines(doc);

                fields.Add(new ConstField(
                    fieldMatch.Groups["name"].Value,
                    fieldMatch.Groups["value"].Value,
                    docLines));
            }

            result.Add(new NestedClass(className, fields, classDocLines));
        }

        return result;

        static string[] GetDocLines(string text)
        {
            return text
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(m => m.TrimStart())
                .Where(m => m.Length > 0)
                .ToArray();
        }
    }
}
