using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace FclEx.SourceGenerator.Sources;

public class UnicodeScalarHelperSource
{
    internal static (string FileName, string Code) Generate()
    {
        const string @namespace = "FclEx.Helpers";
        const string className = "UnicodeScalarHelper";
        const string methodName = "public static partial bool IsEmoji(int unicodeScalar)";

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

        SynchronizationContext.SetSynchronizationContext(null);
        var codes = GetAllEmojiCodes().GetAwaiter().GetResult();

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

    /// <summary>
    /// Gets all emoji unicode values based on unicode.org website.
    /// </summary>
    /// <returns>All emoji unicode values.</returns>
    private static async Task<SortedSet<int>> GetAllEmojiCodes()
    {
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