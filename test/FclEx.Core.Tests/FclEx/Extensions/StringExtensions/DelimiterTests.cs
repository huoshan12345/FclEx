namespace FclEx.Extensions.StringExtensions;

public class DelimiterTests
{
    [Theory]
    [InlineData("SquareBracketed", '[', ']')]
    [InlineData("Parenthesized", '(', ')')]
    [InlineData("CurlyBracketed", '{', '}')]
    [InlineData("AngleBracketed", '<', '>')]
    [InlineData("DoubleQuoted", '"', '"')]
    [InlineData("SingleQuoted", '\'', '\'')]
    [InlineData("BacktickQuoted", '`', '`')]
    [InlineData("SlashDelimited", '/', '/')]
    [InlineData("BackslashDelimited", '\\', '\\')]
    public void Delimiters_InspectAndRemoveOnlyOneOuterPair(string kind, char opening, char closing)
    {
        var pair = $"{opening}{closing}";
        var wrapped = $"{opening} value {closing}";
        var nested = $"{opening}{opening}value{closing}{closing}";

        Assert.True(IsDelimited(kind, pair));
        Assert.Equal("", TrimDelimiters(kind, pair));
        Assert.True(IsDelimited(kind, wrapped));
        Assert.Equal(" value ", TrimDelimiters(kind, wrapped));
        Assert.True(IsDelimited(kind, nested));
        Assert.Equal($"{opening}value{closing}", TrimDelimiters(kind, nested));

        // Interior delimiters and escapes do not affect the boundary check.
        var interior = $"{opening}a{closing}b{opening}c{closing}";
        Assert.True(IsDelimited(kind, interior));
        Assert.Equal($"a{closing}b{opening}c", TrimDelimiters(kind, interior));
        var escaped = $"{opening}value\\{closing}";
        Assert.True(IsDelimited(kind, escaped));
        Assert.Equal("value\\", TrimDelimiters(kind, escaped));

        string[] unmatched =
        [
            "",
            "value",
            opening.ToString(),
            closing.ToString(),
            $"{opening}value",
            $"value{closing}",
            $" {wrapped}",
            $"{wrapped} ",
        ];
        foreach (var str in unmatched)
        {
            Assert.False(IsDelimited(kind, str));
            Assert.Same(str, TrimDelimiters(kind, str));
        }

        if (opening != closing)
        {
            var reversed = $"{closing}value{opening}";
            Assert.False(IsDelimited(kind, reversed));
            Assert.Same(reversed, TrimDelimiters(kind, reversed));
            var mismatched = $"{opening}value!";
            Assert.False(IsDelimited(kind, mismatched));
            Assert.Same(mismatched, TrimDelimiters(kind, mismatched));
        }

        Assert.Throws<NullReferenceException>(() => IsDelimited(kind, null!));
        Assert.Throws<NullReferenceException>(() => TrimDelimiters(kind, null!));
    }

    [Theory]
    [InlineData("SquareBracketed", '[', ']')]
    [InlineData("Parenthesized", '(', ')')]
    [InlineData("CurlyBracketed", '{', '}')]
    [InlineData("AngleBracketed", '<', '>')]
    [InlineData("DoubleQuoted", '"', '"')]
    [InlineData("SingleQuoted", '\'', '\'')]
    [InlineData("BacktickQuoted", '`', '`')]
    [InlineData("SlashDelimited", '/', '/')]
    [InlineData("BackslashDelimited", '\\', '\\')]
    public void WrapDelimiters_AlwaysAddsOnePairAndPreservesContent(string kind, char opening, char closing)
    {
        string[] values =
        [
            "",
            "value",
            " value ",
            $"{opening}value{closing}",
            $"{closing}value{opening}",
            "a\\b\"c'd\r\n中文😀",
        ];
        foreach (var str in values)
        {
            var wrapped = WrapDelimiters(kind, str);
            Assert.Equal($"{opening}{str}{closing}", wrapped);
            Assert.True(IsDelimited(kind, wrapped));
            Assert.Equal(str, TrimDelimiters(kind, wrapped));
        }

        var exception = Assert.Throws<ArgumentNullException>(() => WrapDelimiters(kind, null!));
        Assert.Equal("str", exception.ParamName);
    }

    private static bool IsDelimited(string kind, string str)
    {
        return kind switch
        {
            "SquareBracketed" => str.IsSquareBracketed(),
            "Parenthesized" => str.IsParenthesized(),
            "CurlyBracketed" => str.IsCurlyBracketed(),
            "AngleBracketed" => str.IsAngleBracketed(),
            "DoubleQuoted" => str.IsDoubleQuoted(),
            "SingleQuoted" => str.IsSingleQuoted(),
            "BacktickQuoted" => str.IsBacktickQuoted(),
            "SlashDelimited" => str.IsSlashDelimited(),
            "BackslashDelimited" => str.IsBackslashDelimited(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    private static string TrimDelimiters(string kind, string str)
    {
        return kind switch
        {
            "SquareBracketed" => str.TrimSquareBrackets(),
            "Parenthesized" => str.TrimParentheses(),
            "CurlyBracketed" => str.TrimCurlyBrackets(),
            "AngleBracketed" => str.TrimAngleBrackets(),
            "DoubleQuoted" => str.TrimDoubleQuotes(),
            "SingleQuoted" => str.TrimSingleQuotes(),
            "BacktickQuoted" => str.TrimBackticks(),
            "SlashDelimited" => str.TrimSlashes(),
            "BackslashDelimited" => str.TrimBackslashes(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    private static string WrapDelimiters(string kind, string str)
    {
        return kind switch
        {
            "SquareBracketed" => str.WrapWithSquareBrackets(),
            "Parenthesized" => str.WrapWithParentheses(),
            "CurlyBracketed" => str.WrapWithCurlyBrackets(),
            "AngleBracketed" => str.WrapWithAngleBrackets(),
            "DoubleQuoted" => str.WrapWithDoubleQuotes(),
            "SingleQuoted" => str.WrapWithSingleQuotes(),
            "BacktickQuoted" => str.WrapWithBackticks(),
            "SlashDelimited" => str.WrapWithSlashes(),
            "BackslashDelimited" => str.WrapWithBackslashes(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
