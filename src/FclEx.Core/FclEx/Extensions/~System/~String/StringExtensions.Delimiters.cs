namespace FclEx.Extensions;

partial class StringExtensions
{
    /// <summary>
    /// Determines whether the string has the specified non-overlapping outer delimiters.
    /// </summary>
    /// <remarks>
    /// Checks only the boundaries without trimming whitespace, validating nesting, or interpreting escapes.
    /// Delimiters are matched using <see cref="StringComparison.Ordinal"/>.
    /// A missing closing delimiter uses the opening delimiter; bracket pairs are not inferred.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <param name="open">The non-null, non-empty opening delimiter.</param>
    /// <param name="close">The non-empty closing delimiter, or <see langword="null"/>
    /// to use <paramref name="open"/>.</param>
    /// <returns><see langword="true"/> if both outer delimiters match without overlapping; otherwise,
    /// <see langword="false"/>. A bare pair of delimiters matches.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> or <paramref name="open"/>
    /// is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="open"/> or <paramref name="close"/> is empty.</exception>
    public static bool IsWrappedWith(this string str, string open, string? close = null)
    {
        Check.NotNull(str);
        Check.NotEmpty(open);
        close ??= open;
        Check.NotEmpty(close);

        return str.Length >= open.Length && str.Length - open.Length >= close.Length
            && str.StartsWith(open, StringComparison.Ordinal)
            && str.EndsWith(close, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether the string has the specified non-overlapping outer delimiters.
    /// </summary>
    /// <remarks>
    /// Checks only the boundaries without trimming whitespace, validating nesting, or interpreting escapes.
    /// A missing closing delimiter uses the opening delimiter; bracket pairs are not inferred.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <param name="open">The opening delimiter character.</param>
    /// <param name="close">The closing delimiter character, or <see langword="null"/>
    /// to use <paramref name="open"/>.</param>
    /// <returns><see langword="true"/> if both outer delimiters match without overlapping; otherwise,
    /// <see langword="false"/>. A bare pair of delimiters matches.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/>
    /// is <see langword="null"/>.</exception>
    public static bool IsWrappedWith(this string str, char open, char? close = null)
    {
        Check.NotNull(str);
        return str.Length >= 2 && str[0] == open && str[^1] == (close ?? open);
    }

    /// <summary>
    /// Wraps the string in one new pair of the specified delimiters.
    /// </summary>
    /// <remarks>
    /// Always adds a pair, even if the string is already wrapped. Preserves content without escaping it.
    /// A missing closing delimiter uses the opening delimiter; bracket pairs are not inferred.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <param name="open">The non-null, non-empty opening delimiter.</param>
    /// <param name="close">The non-empty closing delimiter, or <see langword="null"/>
    /// to use <paramref name="open"/>.</param>
    /// <returns>The opening delimiter, original string, and closing delimiter concatenated in that order.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> or <paramref name="open"/>
    /// is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="open"/> or <paramref name="close"/> is empty.</exception>
    public static string WrapWith(this string str, string open, string? close = null)
    {
        Check.NotNull(str);
        Check.NotEmpty(open);
        close ??= open;
        Check.NotEmpty(close);

        return open + str + close;
    }

    /// <summary>
    /// Wraps the string in one new pair of the specified delimiters.
    /// </summary>
    /// <remarks>
    /// Always adds a pair, even if the string is already wrapped. Preserves content without escaping it.
    /// A missing closing delimiter uses the opening delimiter; bracket pairs are not inferred.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <param name="open">The opening delimiter character.</param>
    /// <param name="close">The closing delimiter character, or <see langword="null"/>
    /// to use <paramref name="open"/>.</param>
    /// <returns>The opening delimiter, original string, and closing delimiter concatenated in that order.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/>
    /// is <see langword="null"/>.</exception>
    public static string WrapWith(this string str, char open, char? close = null)
    {
        Check.NotNull(str);
        return open + str + (close ?? open);
    }

    /// <summary>
    /// Removes one matching pair of the specified outer delimiters, if present.
    /// </summary>
    /// <remarks>
    /// Removes at most one outer pair. Preserves inner content and whitespace without interpreting escapes.
    /// Delimiters are matched using <see cref="StringComparison.Ordinal"/>.
    /// A missing closing delimiter uses the opening delimiter; bracket pairs are not inferred.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <param name="open">The non-null, non-empty opening delimiter.</param>
    /// <param name="close">The non-empty closing delimiter, or <see langword="null"/>
    /// to use <paramref name="open"/>.</param>
    /// <returns>The content between matching, non-overlapping delimiters; otherwise, the original string.
    /// A bare pair of delimiters produces an empty string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> or <paramref name="open"/>
    /// is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="open"/> or <paramref name="close"/> is empty.</exception>
    public static string TrimWrapper(this string str, string open, string? close = null)
    {
        close ??= open;
        return str.IsWrappedWith(open, close)
            ? str[open.Length..^close.Length]
            : str;
    }

    /// <summary>
    /// Removes one matching pair of the specified outer delimiters, if present.
    /// </summary>
    /// <remarks>
    /// Removes at most one outer pair. Preserves inner content and whitespace without interpreting escapes.
    /// A missing closing delimiter uses the opening delimiter; bracket pairs are not inferred.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <param name="open">The opening delimiter character.</param>
    /// <param name="close">The closing delimiter character, or <see langword="null"/>
    /// to use <paramref name="open"/>.</param>
    /// <returns>The content between matching, non-overlapping delimiters; otherwise, the original string.
    /// A bare pair of delimiters produces an empty string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/>
    /// is <see langword="null"/>.</exception>
    public static string TrimWrapper(this string str, char open, char? close = null)
    {
        return str.IsWrappedWith(open, close)
            ? str[1..^1]
            : str;
    }

    /// <summary>
    /// Ensures that the string has the specified outer delimiters, adding one pair only when needed.
    /// </summary>
    /// <remarks>
    /// Preserves existing outer pairs and original content without trimming whitespace or interpreting escapes.
    /// Delimiters are matched using <see cref="StringComparison.Ordinal"/>.
    /// A missing closing delimiter uses the opening delimiter; bracket pairs are not inferred.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <param name="open">The non-null, non-empty opening delimiter.</param>
    /// <param name="close">The non-empty closing delimiter, or <see langword="null"/>
    /// to use <paramref name="open"/>.</param>
    /// <returns>The original string if both outer delimiters match without overlapping; otherwise,
    /// the string wrapped in one new pair. An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> or <paramref name="open"/>
    /// is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="open"/> or <paramref name="close"/> is empty.</exception>
    public static string EnsureWrappedWith(this string str, string open, string? close = null)
    {
        return str.IsWrappedWith(open, close)
            ? str
            : str.WrapWith(open, close);
    }

    /// <summary>
    /// Ensures that the string has the specified outer delimiters, adding one pair only when needed.
    /// </summary>
    /// <remarks>
    /// Preserves existing outer pairs and original content without trimming whitespace or interpreting escapes.
    /// A missing closing delimiter uses the opening delimiter; bracket pairs are not inferred.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <param name="open">The opening delimiter character.</param>
    /// <param name="close">The closing delimiter character, or <see langword="null"/>
    /// to use <paramref name="open"/>.</param>
    /// <returns>The original string if both outer delimiters match without overlapping; otherwise,
    /// the string wrapped in one new pair. An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/>
    /// is <see langword="null"/>.</exception>
    public static string EnsureWrappedWith(this string str, char open, char? close = null)
    {
        return str.IsWrappedWith(open, close)
            ? str
            : str.WrapWith(open, close);
    }

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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static bool IsSquareBracketed(this string str)
    {
        return str.IsWrappedWith('[', ']');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string TrimSquareBrackets(this string str)
    {
        return str.TrimWrapper('[', ']');
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
        return str.WrapWith('[', ']');
    }

    /// <summary>
    /// Ensures that the string is enclosed in square brackets, adding one pair only when needed.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Preserves existing outer pairs, inner content,
    /// and whitespace without validating nesting or interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <returns>The original string if already enclosed; otherwise, the string wrapped in one pair.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string EnsureSquareBracketed(this string str)
    {
        return str.EnsureWrappedWith('[', ']');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static bool IsParenthesized(this string str)
    {
        return str.IsWrappedWith('(', ')');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string TrimParentheses(this string str)
    {
        return str.TrimWrapper('(', ')');
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
        return str.WrapWith('(', ')');
    }

    /// <summary>
    /// Ensures that the string is enclosed in parentheses, adding one pair only when needed.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Preserves existing outer pairs, inner content,
    /// and whitespace without validating nesting or interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <returns>The original string if already enclosed; otherwise, the string wrapped in one pair.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string EnsureParenthesized(this string str)
    {
        return str.EnsureWrappedWith('(', ')');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static bool IsCurlyBracketed(this string str)
    {
        return str.IsWrappedWith('{', '}');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string TrimCurlyBrackets(this string str)
    {
        return str.TrimWrapper('{', '}');
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
        return str.WrapWith('{', '}');
    }

    /// <summary>
    /// Ensures that the string is enclosed in curly brackets, adding one pair only when needed.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Preserves existing outer pairs, inner content,
    /// and whitespace without validating nesting or interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <returns>The original string if already enclosed; otherwise, the string wrapped in one pair.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string EnsureCurlyBracketed(this string str)
    {
        return str.EnsureWrappedWith('{', '}');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static bool IsAngleBracketed(this string str)
    {
        return str.IsWrappedWith('<', '>');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string TrimAngleBrackets(this string str)
    {
        return str.TrimWrapper('<', '>');
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
        return str.WrapWith('<', '>');
    }

    /// <summary>
    /// Ensures that the string is enclosed in angle brackets, adding one pair only when needed.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Preserves existing outer pairs, inner content,
    /// and whitespace without validating nesting or interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <returns>The original string if already enclosed; otherwise, the string wrapped in one pair.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string EnsureAngleBracketed(this string str)
    {
        return str.EnsureWrappedWith('<', '>');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static bool IsDoubleQuoted(this string str)
    {
        return str.IsWrappedWith('"', '"');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string TrimDoubleQuotes(this string str)
    {
        return str.TrimWrapper('"', '"');
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
        return str.WrapWith('"', '"');
    }

    /// <summary>
    /// Ensures that the string is enclosed in double quotes, adding one pair only when needed.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Preserves existing outer pairs, inner content,
    /// and whitespace without validating nesting or interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <returns>The original string if already enclosed; otherwise, the string wrapped in one pair.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string EnsureDoubleQuoted(this string str)
    {
        return str.EnsureWrappedWith('"', '"');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static bool IsSingleQuoted(this string str)
    {
        return str.IsWrappedWith('\'', '\'');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string TrimSingleQuotes(this string str)
    {
        return str.TrimWrapper('\'', '\'');
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
        return str.WrapWith('\'', '\'');
    }

    /// <summary>
    /// Ensures that the string is enclosed in single quotes, adding one pair only when needed.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Preserves existing outer pairs, inner content,
    /// and whitespace without validating nesting or interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <returns>The original string if already enclosed; otherwise, the string wrapped in one pair.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string EnsureSingleQuoted(this string str)
    {
        return str.EnsureWrappedWith('\'', '\'');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static bool IsBacktickQuoted(this string str)
    {
        return str.IsWrappedWith('`', '`');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string TrimBackticks(this string str)
    {
        return str.TrimWrapper('`', '`');
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
        return str.WrapWith('`', '`');
    }

    /// <summary>
    /// Ensures that the string is enclosed in backticks, adding one pair only when needed.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Preserves existing outer pairs, inner content,
    /// and whitespace without validating nesting or interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <returns>The original string if already enclosed; otherwise, the string wrapped in one pair.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string EnsureBacktickQuoted(this string str)
    {
        return str.EnsureWrappedWith('`', '`');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static bool IsSlashDelimited(this string str)
    {
        return str.IsWrappedWith('/', '/');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string TrimSlashes(this string str)
    {
        return str.TrimWrapper('/', '/');
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
        return str.WrapWith('/', '/');
    }

    /// <summary>
    /// Ensures that the string is enclosed in forward slashes, adding one pair only when needed.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Preserves existing outer pairs, inner content,
    /// and whitespace without validating nesting or interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <returns>The original string if already enclosed; otherwise, the string wrapped in one pair.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string EnsureSlashDelimited(this string str)
    {
        return str.EnsureWrappedWith('/', '/');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static bool IsBackslashDelimited(this string str)
    {
        return str.IsWrappedWith('\\', '\\');
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
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string TrimBackslashes(this string str)
    {
        return str.TrimWrapper('\\', '\\');
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
        return str.WrapWith('\\', '\\');
    }

    /// <summary>
    /// Ensures that the string is enclosed in backslashes, adding one pair only when needed.
    /// </summary>
    /// <remarks>
    /// Checks only the first and last characters. Preserves existing outer pairs, inner content,
    /// and whitespace without validating nesting or interpreting escapes.
    /// </remarks>
    /// <param name="str">The non-null string to process. May be empty.</param>
    /// <returns>The original string if already enclosed; otherwise, the string wrapped in one pair.
    /// An empty string produces a bare pair of delimiters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
    public static string EnsureBackslashDelimited(this string str)
    {
        return str.EnsureWrappedWith('\\', '\\');
    }
}
