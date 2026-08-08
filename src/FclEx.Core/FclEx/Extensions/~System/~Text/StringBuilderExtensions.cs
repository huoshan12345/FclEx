namespace FclEx.Extensions;

public static partial class StringBuilderExtensions
{
    public static StringBuilder AppendIf(this StringBuilder builder, object? value, bool condition)
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
    /// <see langword="true"/> if the substring of the <see cref="StringBuilder"/> matches the given span;
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
    /// <see langword="true"/> if the <see cref="StringBuilder"/> starts with the specified span;
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
    /// <see langword="true"/> if the <see cref="StringBuilder"/> ends with the specified span;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool EndsWith(this StringBuilder builder, ReadOnlySpan<char> span)
    {
        var startIndex = builder.Length - span.Length;
        return startIndex >= 0 && builder.Equals(span, startIndex);
    }


#if !NET5_0_OR_GREATER
    public static StringBuilder AppendJoin<T>(this StringBuilder builder, string? separator, IEnumerable<T> values)
    {
        Check.NotNull(builder);
        Check.NotNull(values);

        using var e = values.GetEnumerator();

        if (!e.MoveNext())
            return builder;

        builder.Append(e.Current);

        while (e.MoveNext())
        {
            builder.Append(separator);
            builder.Append(e.Current);
        }

        return builder;
    }
#endif
}