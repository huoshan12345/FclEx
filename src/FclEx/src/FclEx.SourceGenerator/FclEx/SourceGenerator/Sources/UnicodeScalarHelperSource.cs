using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.CodeAnalysis.Diagnostics;
#pragma warning disable RS1035

namespace FclEx.SourceGenerator.Sources;

internal static class UnicodeScalarHelperSource
{
    private static readonly bool IsGithubAction = Environment.GetEnvironmentVariable("GITHUB_ACTION") is { Length: > 0 };

    internal static SourceInfo Generate(SourceProductionContext context, AnalyzerConfigOptionsProvider options)
    {
        const string @namespace = "FclEx.Helpers";
        const string className = "UnicodeScalarHelper";
        const string methodName = "public static partial bool IsEmoji(int unicodeScalar)";

        SynchronizationContext.SetSynchronizationContext(null);
        var codes = GetAllEmojiCodes(context, options).GetAwaiter().GetResult();

        if (codes is null)
            return SourceInfo.Failed;

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine($"partial class {className}")
            .WriteOpeningBracket();

        builder.WriteLine(methodName)
            .WriteOpeningBracket();

        builder.WriteLine("switch (unicodeScalar)");
        builder.WriteOpeningBracket();

        foreach (var code in codes)
        {
            builder.WriteLine($"case {code}:");
        }

        builder.WriteOpeningBracket();
        builder.WriteLine("return true;");
        builder.WriteClosingBracket();
        builder.WriteLine("default:");
        builder.WriteOpeningBracket();
        builder.WriteLine("return false;");
        builder.WriteClosingBracket();

        // switch
        builder.WriteClosingBracket();

        // method
        builder.WriteClosingBracket();

        // End class declaration
        builder.WriteClosingBracket();

        // End namespace declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }

    private static async Task<SortedSet<int>?> GetAllEmojiCodes(SourceProductionContext context, AnalyzerConfigOptionsProvider options)
    {
        await Task.Yield();

        const string key = "build_property.projectdir";
        var path = options.GetGlobalOption(key);
        if (path is null)
        {
            Report("Cannot find global option by key '{0}'", key);
            return null;
        }

        var index = path.IndexOf("src", StringComparison.Ordinal);
        if (index < 0)
        {
            Report("Cannot locate src directory from current path: {0}", path);
            return null;
        }

        var assembly = typeof(UnicodeScalarHelperSource).Assembly.GetName().Name;
        var projectDir = Path.Combine(path[..index], "src", "FclEx", "src", assembly);
        if (Directory.Exists(projectDir) == false)
        {
            Report("Source generator project directory does not exist: {0}", projectDir);
            return null;
        }

        var resourcesDir = Path.Combine(projectDir, "Resources");
        var file = new FileInfo(Path.Combine(resourcesDir, "emoji-codes.txt"));
        if (file.Exists)
        {
            // file is updated within 7 days
            // Or it is running under GitHub action
            if (file.LastWriteTimeUtc > DateTime.UtcNow.AddDays(-7) || IsGithubAction)
            {
                var content = File.ReadAllText(file.FullName);
                var lines = content.Split('\r', '\n')
                    .Select(m => m.Trim())
                    .Where(m => m.Length > 0)
                    .Select(int.Parse);

                var set = new SortedSet<int>(lines);
                if (set.Count > 0)
                    return set;
            }
        }

        var codes = await FetchAllEmojiCodes();

        if (Directory.Exists(resourcesDir) == false)
            Directory.CreateDirectory(resourcesDir);

        using var writer = new StreamWriter(file.FullName, false);
        foreach (var code in codes)
        {
            writer.WriteLine(code);
        }

        return codes;

        void Report(string messageFormat, params object?[]? args)
        {
            var descriptor = new DiagnosticDescriptor(
                id: "FclEx",
                title: nameof(GetAllEmojiCodes),
                messageFormat: messageFormat,
                category: nameof(UnicodeScalarHelperSource),
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);
            context.ReportDiagnostic(Diagnostic.Create(descriptor, null, messageArgs: args));
        }
    }

    /// <summary>
    /// Gets all emoji unicode values based on unicode.org website.
    /// </summary>
    /// <returns>All emoji unicode values.</returns>
    private static async Task<SortedSet<int>> FetchAllEmojiCodes()
    {
        await Task.Yield();

        using var httpClient = new HttpClient();

        // the official website: https://unicode.org/emoji/charts/full-emoji-list.html
        // which is very slow, so we use a mirror one.
        var text = await httpClient.GetStringAsync("https://unicode-org.github.io/emoji/emoji/charts-16.0/emoji-list.html");

        var htmlParser = new HtmlParser();
        var htmlDocument = htmlParser.ParseDocument(text);

        var emojiSet = new SortedSet<int>();
        // HTML above contains elements like 
        // <td class='code'><a href='#1f600' name='1f600'>U+1F600</a></td>
        foreach (var emojiCodeElement in htmlDocument.All.Where(element => element is { ClassName: "code", NodeName: "TD", NodeType: NodeType.Element }))
        {
            // Get us U+1F600
            var unicodeRepresentation = emojiCodeElement.TextContent;

            // Convert into Hex representation
            unicodeRepresentation = unicodeRepresentation.Replace("U+", "");

            // Certain emojis are a combination of multiple
            // For example U+1F636 U+200D U+1F32B U+FE0F
            // U+200D -  Zero Width Joiner
            // U+FEOF - Variation Selector-16
            foreach (var unicodeEmojiSplit in unicodeRepresentation.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                emojiSet.Add(int.Parse(unicodeEmojiSplit, NumberStyles.HexNumber));
            }
        }

        return emojiSet;
    }

}