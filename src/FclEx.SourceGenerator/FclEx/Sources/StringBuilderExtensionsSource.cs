namespace FclEx.Sources;

internal class StringBuilderExtensionsSource
{
    private static readonly string[] _usings =
    [
        "System",
        "System.Text",
    ];

    private static readonly (string name, char openingQuote, char closingQuote, string commentForValue, string commentForAction)[] _methods =
    [
        ("AppendSingleQuoted", '\'', '\'', "Appends the specified value enclosed in single quotes.", "Appends content enclosed in single quotes."),
        ("AppendDoubleQuoted", '"', '"', "Appends the specified value enclosed in double quotes.", "Appends content enclosed in double quotes."),
        ("AppendParenthesized", '(', ')', "Appends the specified value enclosed in parentheses.", "Appends content enclosed in parentheses."),
        ("AppendSquareBracketed", '[', ']', "Appends the specified value enclosed in square brackets.", "Appends content enclosed in square brackets."),
        ("AppendCurlyBraced", '{', '}', "Appends the specified value enclosed in curly braces.", "Appends content enclosed in curly braces."),
        ("AppendAngleBracketed", '<', '>', "Appends the specified value enclosed in angle brackets.", "Appends content enclosed in angle brackets."),
        ("AppendBackticked", '`', '`', "Appends the specified value enclosed in backticks.", "Appends content enclosed in backticks."),
    ];

    internal static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Extensions";
        const string className = "StringBuilderExtensions";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteEnableNullable()
            .WriteLine()
            .WriteUsings(_usings)
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine($"public static partial class {className}")
            .WriteOpeningBracket();

        foreach (var (name, openingQuote, closingQuote, commentForValue, commentForAction) in _methods)
        {
            var opening = EscapeChar(openingQuote);
            var closing = EscapeChar(closingQuote);

            builder.WriteSummary(commentForValue);
            builder.WriteLine($"public static StringBuilder {name}(this StringBuilder builder, string? value)");
            builder.WriteOpeningBracket();
            builder.WriteLine($"return builder.AppendWrapped('{opening}', value, '{closing}');");
            builder.WriteClosingBracket();
            builder.WriteLine();

            builder.WriteSummary(commentForAction);
            builder.WriteLine($"public static StringBuilder {name}(this StringBuilder builder, Action<StringBuilder> appendContent)");
            builder.WriteOpeningBracket();
            builder.WriteLine($"return builder.AppendWrapped('{opening}', appendContent, '{closing}');");
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

    private static string EscapeChar(char c)
    {
        return c switch
        {
            '\'' => "\\'",
            _ => c.ToString()
        };
    }
}
