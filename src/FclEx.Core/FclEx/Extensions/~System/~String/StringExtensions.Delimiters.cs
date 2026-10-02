namespace FclEx.Extensions;

partial class StringExtensions
{
    /// <summary>
    /// Determines whether the string is enclosed in square brackets.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Does not trim whitespace or validate
    /// nesting, escaping, or the content between the delimiters. An empty enclosed string matches.
    /// </remarks>
    /// <param name="str">The non-null string to check.</param>
    /// <returns><see langword="true"/> if the string has at least two characters and begins and ends
    /// with the corresponding delimiters; otherwise, <see langword="false"/>.</returns>
    public static bool IsSquareBracketed(this string str)
    {
        return str.Length >= 2 && str.StartsWith('[') && str.EndsWith(']');
    }

    /// <summary>
    /// Removes one pair of surrounding square brackets, if present.
    /// </summary>
    /// <remarks>
    /// Removes only the first and last characters when <see cref="IsSquareBracketed"/> returns
    /// <see langword="true"/>. Preserves inner delimiters and whitespace without interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process.</param>
    /// <returns>The content between the outer delimiters, including an empty string for a bare pair,
    /// or the original string if it is not enclosed in the corresponding delimiters.</returns>
    public static string TrimSquareBrackets(this string str)
    {
        return str.IsSquareBracketed()
            ? str[1..^1]
            : str;
    }

    /// <summary>
    /// Wraps the string in one new pair of square brackets.
    /// </summary>
    /// <remarks>
    /// Always adds a pair, even when the string already has surrounding delimiters.
    /// Preserves the original content and whitespace without escaping or validating it.
    /// </remarks>
    /// <param name="str">The non-null string to wrap. May be empty.</param>
    /// <returns>The string preceded by the opening delimiter and followed by the closing delimiter.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string WrapWithSquareBrackets(this string str)
    {
        Check.NotNull(str);
        return "[" + str + "]";
    }

    /// <summary>
    /// Determines whether the string is enclosed in parentheses.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Does not trim whitespace or validate
    /// nesting, escaping, or the content between the delimiters. An empty enclosed string matches.
    /// </remarks>
    /// <param name="str">The non-null string to check.</param>
    /// <returns><see langword="true"/> if the string has at least two characters and begins and ends
    /// with the corresponding delimiters; otherwise, <see langword="false"/>.</returns>
    public static bool IsParenthesized(this string str)
    {
        return str.Length >= 2 && str.StartsWith('(') && str.EndsWith(')');
    }

    /// <summary>
    /// Removes one pair of surrounding parentheses, if present.
    /// </summary>
    /// <remarks>
    /// Removes only the first and last characters when <see cref="IsParenthesized"/> returns
    /// <see langword="true"/>. Preserves inner delimiters and whitespace without interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process.</param>
    /// <returns>The content between the outer delimiters, including an empty string for a bare pair,
    /// or the original string if it is not enclosed in the corresponding delimiters.</returns>
    public static string TrimParentheses(this string str)
    {
        return str.IsParenthesized()
            ? str[1..^1]
            : str;
    }

    /// <summary>
    /// Wraps the string in one new pair of parentheses.
    /// </summary>
    /// <remarks>
    /// Always adds a pair, even when the string already has surrounding delimiters.
    /// Preserves the original content and whitespace without escaping or validating it.
    /// </remarks>
    /// <param name="str">The non-null string to wrap. May be empty.</param>
    /// <returns>The string preceded by the opening delimiter and followed by the closing delimiter.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string WrapWithParentheses(this string str)
    {
        Check.NotNull(str);
        return "(" + str + ")";
    }

    /// <summary>
    /// Determines whether the string is enclosed in curly brackets.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Does not trim whitespace or validate
    /// nesting, escaping, or the content between the delimiters. An empty enclosed string matches.
    /// </remarks>
    /// <param name="str">The non-null string to check.</param>
    /// <returns><see langword="true"/> if the string has at least two characters and begins and ends
    /// with the corresponding delimiters; otherwise, <see langword="false"/>.</returns>
    public static bool IsCurlyBracketed(this string str)
    {
        return str.Length >= 2 && str.StartsWith('{') && str.EndsWith('}');
    }

    /// <summary>
    /// Removes one pair of surrounding curly brackets, if present.
    /// </summary>
    /// <remarks>
    /// Removes only the first and last characters when <see cref="IsCurlyBracketed"/> returns
    /// <see langword="true"/>. Preserves inner delimiters and whitespace without interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process.</param>
    /// <returns>The content between the outer delimiters, including an empty string for a bare pair,
    /// or the original string if it is not enclosed in the corresponding delimiters.</returns>
    public static string TrimCurlyBrackets(this string str)
    {
        return str.IsCurlyBracketed()
            ? str[1..^1]
            : str;
    }

    /// <summary>
    /// Wraps the string in one new pair of curly brackets.
    /// </summary>
    /// <remarks>
    /// Always adds a pair, even when the string already has surrounding delimiters.
    /// Preserves the original content and whitespace without escaping or validating it.
    /// </remarks>
    /// <param name="str">The non-null string to wrap. May be empty.</param>
    /// <returns>The string preceded by the opening delimiter and followed by the closing delimiter.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string WrapWithCurlyBrackets(this string str)
    {
        Check.NotNull(str);
        return "{" + str + "}";
    }

    /// <summary>
    /// Determines whether the string is enclosed in angle brackets.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Does not trim whitespace or validate
    /// nesting, escaping, or the content between the delimiters. An empty enclosed string matches.
    /// </remarks>
    /// <param name="str">The non-null string to check.</param>
    /// <returns><see langword="true"/> if the string has at least two characters and begins and ends
    /// with the corresponding delimiters; otherwise, <see langword="false"/>.</returns>
    public static bool IsAngleBracketed(this string str)
    {
        return str.Length >= 2 && str.StartsWith('<') && str.EndsWith('>');
    }

    /// <summary>
    /// Removes one pair of surrounding angle brackets, if present.
    /// </summary>
    /// <remarks>
    /// Removes only the first and last characters when <see cref="IsAngleBracketed"/> returns
    /// <see langword="true"/>. Preserves inner delimiters and whitespace without interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process.</param>
    /// <returns>The content between the outer delimiters, including an empty string for a bare pair,
    /// or the original string if it is not enclosed in the corresponding delimiters.</returns>
    public static string TrimAngleBrackets(this string str)
    {
        return str.IsAngleBracketed()
            ? str[1..^1]
            : str;
    }

    /// <summary>
    /// Wraps the string in one new pair of angle brackets.
    /// </summary>
    /// <remarks>
    /// Always adds a pair, even when the string already has surrounding delimiters.
    /// Preserves the original content and whitespace without escaping or validating it.
    /// </remarks>
    /// <param name="str">The non-null string to wrap. May be empty.</param>
    /// <returns>The string preceded by the opening delimiter and followed by the closing delimiter.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string WrapWithAngleBrackets(this string str)
    {
        Check.NotNull(str);
        return "<" + str + ">";
    }

    /// <summary>
    /// Determines whether the string is enclosed in double quotes.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Does not trim whitespace or validate
    /// nesting, escaping, or the content between the delimiters. An empty enclosed string matches.
    /// </remarks>
    /// <param name="str">The non-null string to check.</param>
    /// <returns><see langword="true"/> if the string has at least two characters and begins and ends
    /// with the corresponding delimiters; otherwise, <see langword="false"/>.</returns>
    public static bool IsDoubleQuoted(this string str)
    {
        return str.Length >= 2 && str.StartsWith('"') && str.EndsWith('"');
    }

    /// <summary>
    /// Removes one pair of surrounding double quotes, if present.
    /// </summary>
    /// <remarks>
    /// Removes only the first and last characters when <see cref="IsDoubleQuoted"/> returns
    /// <see langword="true"/>. Preserves inner delimiters and whitespace without interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process.</param>
    /// <returns>The content between the outer delimiters, including an empty string for a bare pair,
    /// or the original string if it is not enclosed in the corresponding delimiters.</returns>
    public static string TrimDoubleQuotes(this string str)
    {
        return str.IsDoubleQuoted()
            ? str[1..^1]
            : str;
    }

    /// <summary>
    /// Wraps the string in one new pair of double quotes.
    /// </summary>
    /// <remarks>
    /// Always adds a pair, even when the string already has surrounding delimiters.
    /// Preserves the original content and whitespace without escaping or validating it.
    /// </remarks>
    /// <param name="str">The non-null string to wrap. May be empty.</param>
    /// <returns>The string preceded by the opening delimiter and followed by the closing delimiter.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string WrapWithDoubleQuotes(this string str)
    {
        Check.NotNull(str);
        return "\"" + str + "\"";
    }

    /// <summary>
    /// Determines whether the string is enclosed in single quotes.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Does not trim whitespace or validate
    /// nesting, escaping, or the content between the delimiters. An empty enclosed string matches.
    /// </remarks>
    /// <param name="str">The non-null string to check.</param>
    /// <returns><see langword="true"/> if the string has at least two characters and begins and ends
    /// with the corresponding delimiters; otherwise, <see langword="false"/>.</returns>
    public static bool IsSingleQuoted(this string str)
    {
        return str.Length >= 2 && str.StartsWith('\'') && str.EndsWith('\'');
    }

    /// <summary>
    /// Removes one pair of surrounding single quotes, if present.
    /// </summary>
    /// <remarks>
    /// Removes only the first and last characters when <see cref="IsSingleQuoted"/> returns
    /// <see langword="true"/>. Preserves inner delimiters and whitespace without interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process.</param>
    /// <returns>The content between the outer delimiters, including an empty string for a bare pair,
    /// or the original string if it is not enclosed in the corresponding delimiters.</returns>
    public static string TrimSingleQuotes(this string str)
    {
        return str.IsSingleQuoted()
            ? str[1..^1]
            : str;
    }

    /// <summary>
    /// Wraps the string in one new pair of single quotes.
    /// </summary>
    /// <remarks>
    /// Always adds a pair, even when the string already has surrounding delimiters.
    /// Preserves the original content and whitespace without escaping or validating it.
    /// </remarks>
    /// <param name="str">The non-null string to wrap. May be empty.</param>
    /// <returns>The string preceded by the opening delimiter and followed by the closing delimiter.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string WrapWithSingleQuotes(this string str)
    {
        Check.NotNull(str);
        return "'" + str + "'";
    }

    /// <summary>
    /// Determines whether the string is enclosed in backticks.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Does not trim whitespace or validate
    /// nesting, escaping, or the content between the delimiters. An empty enclosed string matches.
    /// </remarks>
    /// <param name="str">The non-null string to check.</param>
    /// <returns><see langword="true"/> if the string has at least two characters and begins and ends
    /// with the corresponding delimiters; otherwise, <see langword="false"/>.</returns>
    public static bool IsBacktickQuoted(this string str)
    {
        return str.Length >= 2 && str.StartsWith('`') && str.EndsWith('`');
    }

    /// <summary>
    /// Removes one pair of surrounding backticks, if present.
    /// </summary>
    /// <remarks>
    /// Removes only the first and last characters when <see cref="IsBacktickQuoted"/> returns
    /// <see langword="true"/>. Preserves inner delimiters and whitespace without interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process.</param>
    /// <returns>The content between the outer delimiters, including an empty string for a bare pair,
    /// or the original string if it is not enclosed in the corresponding delimiters.</returns>
    public static string TrimBackticks(this string str)
    {
        return str.IsBacktickQuoted()
            ? str[1..^1]
            : str;
    }

    /// <summary>
    /// Wraps the string in one new pair of backticks.
    /// </summary>
    /// <remarks>
    /// Always adds a pair, even when the string already has surrounding delimiters.
    /// Preserves the original content and whitespace without escaping or validating it.
    /// </remarks>
    /// <param name="str">The non-null string to wrap. May be empty.</param>
    /// <returns>The string preceded by the opening delimiter and followed by the closing delimiter.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string WrapWithBackticks(this string str)
    {
        Check.NotNull(str);
        return "`" + str + "`";
    }

    /// <summary>
    /// Determines whether the string is enclosed in forward slashes.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Does not trim whitespace or validate
    /// nesting, escaping, or the content between the delimiters. An empty enclosed string matches.
    /// </remarks>
    /// <param name="str">The non-null string to check.</param>
    /// <returns><see langword="true"/> if the string has at least two characters and begins and ends
    /// with the corresponding delimiters; otherwise, <see langword="false"/>.</returns>
    public static bool IsSlashDelimited(this string str)
    {
        return str.Length >= 2 && str.StartsWith('/') && str.EndsWith('/');
    }

    /// <summary>
    /// Removes one pair of surrounding forward slashes, if present.
    /// </summary>
    /// <remarks>
    /// Removes only the first and last characters when <see cref="IsSlashDelimited"/> returns
    /// <see langword="true"/>. Preserves inner delimiters and whitespace without interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process.</param>
    /// <returns>The content between the outer delimiters, including an empty string for a bare pair,
    /// or the original string if it is not enclosed in the corresponding delimiters.</returns>
    public static string TrimSlashes(this string str)
    {
        return str.IsSlashDelimited()
            ? str[1..^1]
            : str;
    }

    /// <summary>
    /// Wraps the string in one new pair of forward slashes.
    /// </summary>
    /// <remarks>
    /// Always adds a pair, even when the string already has surrounding delimiters.
    /// Preserves the original content and whitespace without escaping or validating it.
    /// </remarks>
    /// <param name="str">The non-null string to wrap. May be empty.</param>
    /// <returns>The string preceded by the opening delimiter and followed by the closing delimiter.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string WrapWithSlashes(this string str)
    {
        Check.NotNull(str);
        return "/" + str + "/";
    }

    /// <summary>
    /// Determines whether the string is enclosed in backslashes.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Does not trim whitespace or validate
    /// nesting, escaping, or the content between the delimiters. An empty enclosed string matches.
    /// </remarks>
    /// <param name="str">The non-null string to check.</param>
    /// <returns><see langword="true"/> if the string has at least two characters and begins and ends
    /// with the corresponding delimiters; otherwise, <see langword="false"/>.</returns>
    public static bool IsBackslashDelimited(this string str)
    {
        return str.Length >= 2 && str.StartsWith('\\') && str.EndsWith('\\');
    }

    /// <summary>
    /// Removes one pair of surrounding backslashes, if present.
    /// </summary>
    /// <remarks>
    /// Removes only the first and last characters when <see cref="IsBackslashDelimited"/> returns
    /// <see langword="true"/>. Preserves inner delimiters and whitespace without interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process.</param>
    /// <returns>The content between the outer delimiters, including an empty string for a bare pair,
    /// or the original string if it is not enclosed in the corresponding delimiters.</returns>
    public static string TrimBackslashes(this string str)
    {
        return str.IsBackslashDelimited()
            ? str[1..^1]
            : str;
    }

    /// <summary>
    /// Wraps the string in one new pair of backslashes.
    /// </summary>
    /// <remarks>
    /// Always adds a pair, even when the string already has surrounding delimiters.
    /// Preserves the original content and whitespace without escaping or validating it.
    /// </remarks>
    /// <param name="str">The non-null string to wrap. May be empty.</param>
    /// <returns>The string preceded by the opening delimiter and followed by the closing delimiter.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string WrapWithBackslashes(this string str)
    {
        Check.NotNull(str);
        return "\\" + str + "\\";
    }
}
