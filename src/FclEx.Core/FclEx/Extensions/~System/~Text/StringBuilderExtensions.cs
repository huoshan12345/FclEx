namespace FclEx.Extensions;

public static class StringBuilderExtensions
{
    /// <summary>
    /// Appends a quoted value to the provided StringBuilder.
    /// </summary>
    /// <param name="builder">The StringBuilder to which the quoted value will be appended.</param>
    /// <param name="openingQuote">The character used to open the quote.</param>
    /// <param name="valueAction">An action that modifies the StringBuilder to include the value to be quoted. 
    /// Note: Character escaping should be considered when implementing this action.</param>
    /// <param name="closingQuote">The character used to close the quote. If not provided, the same character as openingQuote will be used.</param>
    /// <returns>The updated StringBuilder instance.</returns>
    public static StringBuilder AppendQuoted(this StringBuilder builder, string openingQuote, Action<StringBuilder> valueAction, string? closingQuote = null)
    {
        closingQuote ??= openingQuote;
        builder.Append(openingQuote);
        valueAction(builder);
        builder.Append(closingQuote);
        return builder;
    }

    /// <summary>
    /// Appends a quoted string to the provided <see cref="StringBuilder"/> instance. 
    /// The method wraps the input value in quotes, escapes any instances of the quote characters 
    /// and the escape character within the string, and returns the updated <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> to which the quoted string is appended.</param>
    /// <param name="openingQuote">The character used as the starting quote.</param>
    /// <param name="value">The string value to be quoted and appended.</param>
    /// <param name="closingQuote">
    /// Optional. The character used as the closing quote. If not provided, the same character as <paramref name="openingQuote"/> is used.
    /// </param>
    /// <param name="escapeCharacter">Optional. The character used to escape any quotes or escape characters found in <paramref name="value"/>. Default is '\\'.</param>
    /// <returns>The updated <see cref="StringBuilder"/> with the quoted and escaped string appended.</returns>
    public static StringBuilder AppendQuoted(this StringBuilder builder, char openingQuote, string value, char? closingQuote = null, char escapeCharacter = '\\')
    {
        closingQuote ??= openingQuote;
        builder.Append(openingQuote);

        foreach (var ch in value)
        {
            // Escape any quotes, closing quotes, or escape characters in the string
            if (ch == openingQuote || ch == closingQuote || ch == escapeCharacter)
            {
                builder.Append(escapeCharacter);
            }
            builder.Append(ch);  // Append the actual character
        }

        builder.Append(closingQuote);
        return builder;
    }

    /// <summary>
    /// Appends a string value enclosed in single quotes to the provided <see cref="StringBuilder"/> instance.
    /// The method also escapes any single quotes or the specified escape character within the string.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> to which the single-quoted string is appended.</param>
    /// <param name="value">The string value to be enclosed in single quotes.</param>
    /// <param name="escapeCharacter">
    /// Optional. The character used to escape any single quotes or escape characters found in <paramref name="value"/>. 
    /// Default is '\\'.
    /// </param>
    /// <returns>The updated <see cref="StringBuilder"/> with the appended single-quoted string.</returns>
    public static StringBuilder AppendSingleQuoted(this StringBuilder builder, string value, char escapeCharacter = '\\')
    {
        return builder.AppendQuoted('\'', value, null, escapeCharacter);
    }

    /// <summary>
    /// Appends a line feed character (LF, \n) to the provided <see cref="StringBuilder"/> instance.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> to which the line feed character is appended.</param>
    /// <returns>The updated <see cref="StringBuilder"/> with the appended line feed character.</returns>
    public static StringBuilder AppendLineFeed(this StringBuilder builder)
    {
        return builder.Append('\n');
    }

    /// <summary>
    /// Appends a carriage return character (CR, \r) to the provided <see cref="StringBuilder"/> instance.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> to which the line feed character is appended.</param>
    /// <returns>The updated <see cref="StringBuilder"/> with the appended line feed character.</returns>
    public static StringBuilder AppendCarriageReturn(this StringBuilder builder)
    {
        return builder.Append('\r');
    }

    /// <summary>
    /// Appends a value enclosed in parentheses to the <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> to append to.</param>
    /// <param name="valueAction">
    /// An action that writes the content to be enclosed in parentheses into the <see cref="StringBuilder"/>.
    /// </param>
    /// <returns>The modified <see cref="StringBuilder"/> instance.</returns>
    public static StringBuilder AppendParenthesized(this StringBuilder builder, Action<StringBuilder> valueAction)
    {
        return builder.AppendQuoted("(", valueAction, ")");
    }

#if NETSTANDARD2_0
    public static StringBuilder AppendJoin<T>(this StringBuilder builder, string? separator, IEnumerable<T> values)
    {
        Check.NotNull(builder);
        Check.NotNull(values);

        using var e = values.GetEnumerator();

        if (!e.MoveNext())
            return builder;

        builder.Append(e.Current?.ToString());

        while (e.MoveNext())
        {
            builder.Append(separator);
            builder.Append(e.Current?.ToString());
        }

        return builder;
    }
#endif

    public static StringBuilder AppendIf(this StringBuilder builder, string? value, bool condition)
    {
        if (condition)
            builder.Append(value);
        return builder;
    }

    public static StringBuilder AppendLine(this StringBuilder builder, object? value)
    {
        return builder.AppendLine(value?.ToString());
    }

    /// <summary>
    /// Appends the specified text to the StringBuilder if the total length does not exceed the specified limit.
    /// </summary>
    /// <param name="builder">The StringBuilder to which the text will be appended.</param>
    /// <param name="text">The text to append to the StringBuilder.</param>
    /// <param name="limit">The maximum allowed length after appending the text.</param>
    /// <returns>True if the text was appended successfully; otherwise, false if the length exceeds the limit.</returns>
    public static bool AppendLimited(this StringBuilder builder, string text, int limit)
    {
        if (builder.Length + text.Length > limit)
            return false;

        builder.Append(text);
        return true;
    }

    /// <summary>
    /// Determines whether a substring of the <see cref="StringBuilder"/> content 
    /// starting at the specified index is equal to the given span of characters.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> instance to compare.</param>
    /// <param name="span">The character span to compare with the substring.</param>
    /// <param name="startIndex">The zero-based starting index in the <see cref="StringBuilder"/>.</param>
    /// <returns>
    /// <c>true</c> if the substring of the <see cref="StringBuilder"/> matches the given span;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="startIndex"/> is less than zero.
    /// </exception>
    public static bool Equals(this StringBuilder builder, ReadOnlySpan<char> span, int startIndex)
    {
        Check.NotNull(builder);
        Check.NotLessThan(startIndex, 0);

        if (builder.Length < span.Length + startIndex)
            return false;

        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] != builder[startIndex + i])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Determines whether the beginning of the <see cref="StringBuilder"/> 
    /// matches the specified span of characters.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> instance to check.</param>
    /// <param name="span">The character span to compare to the start of the <see cref="StringBuilder"/>.</param>
    /// <returns>
    /// <c>true</c> if the <see cref="StringBuilder"/> starts with the specified span;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool StartsWith(this StringBuilder builder, ReadOnlySpan<char> span)
    {
        return builder.Equals(span, 0);
    }

    /// <summary>
    /// Determines whether the end of the <see cref="StringBuilder"/> 
    /// matches the specified span of characters.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> instance to check.</param>
    /// <param name="span">The character span to compare to the end of the <see cref="StringBuilder"/>.</param>
    /// <returns>
    /// <c>true</c> if the <see cref="StringBuilder"/> ends with the specified span;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool EndsWith(this StringBuilder builder, ReadOnlySpan<char> span)
    {
        var startIndex = builder.Length - span.Length;
        return startIndex >= 0 && builder.Equals(span, startIndex);
    }
}