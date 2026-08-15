namespace FclEx.Extensions;

partial class StringExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str) => string.IsNullOrEmpty(str);

    [MethodImpl(AggressiveInlining)]
    public static bool IsNotEmpty([NotNullWhen(true)] this string? str) => str.IsNullOrEmpty() == false;

    [MethodImpl(AggressiveInlining)]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str) => string.IsNullOrWhiteSpace(str);

    [MethodImpl(AggressiveInlining)]
    public static string Format(this string str, params object[] args) => string.Format(str, args);

    [MethodImpl(AggressiveInlining)]
    public static byte[] ToUtf8Bytes(this string input) => input.ToBytes(Encoding.UTF8);

    [MethodImpl(AggressiveInlining)]
    public static byte[] ToBytes(this string input, Encoding? encoding = null) => (encoding ?? Encoding.UTF8).GetBytes(input);

    [MethodImpl(AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? UrlEncode(this string? value) => HttpUtility.UrlEncode(value);

    [MethodImpl(AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? UrlDecode(this string? value) => HttpUtility.UrlDecode(value);

    [MethodImpl(AggressiveInlining)]
    public static string UriEscape(this string value) => Uri.EscapeDataString(value);

    [MethodImpl(AggressiveInlining)]
    public static string UriUnescape(this string value) => Uri.UnescapeDataString(value);

    public static string Truncate(this string? str, int maxLength, bool appendTrailingDots = true)
    {
        if (maxLength <= 0)
            return string.Empty;

        if (str.IsNullOrEmpty() || maxLength >= str.Length)
            return str ?? string.Empty;

        var sub = str[..maxLength];
        return appendTrailingDots
            ? sub + "..."
            : sub;
    }

    /// <summary>
    /// Performs a cheap, conservative precheck for whether <paramref name="text"/> could be an XML document.
    /// </summary>
    /// <remarks>
    /// A <see langword="false"/> result means the text cannot be a well-formed XML document. A
    /// <see langword="true"/> result does not establish well-formedness; use an XML parser when validation is required.
    /// This method only checks the document envelope and does not inspect elements, attributes, entities, or nesting.
    /// </remarks>
    public static bool CouldBeXmlDocument([NotNullWhen(true)] this string? text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var value = text!;
        var start = 0;
        var end = value.Length - 1;

        while (start <= end && char.IsWhiteSpace(value[start]))
            start++;

        if (start <= end && value[start] == '\uFEFF')
            start++;

        while (start <= end && char.IsWhiteSpace(value[start]))
            start++;

        while (end >= start && char.IsWhiteSpace(value[end]))
            end--;

        return start <= end && value[start] == '<' && value[end] == '>';
    }

    /// <summary>
    /// Converts a hexadecimal string representation into a byte array.
    /// </summary>
    /// <param name="hex">The hexadecimal string to convert. This string must have an even number of characters.</param>
    /// <returns>A byte array representing the binary data encoded in the hexadecimal string.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the input string has an odd number of characters, as this is not a valid hexadecimal representation.
    /// </exception>
    /// <remarks>
    /// This method first checks if the input string is <see langword="null"/> and validates the length. 
    /// If the string is valid, it processes each pair of characters, converting them into their 
    /// corresponding byte values. The method supports both uppercase and lowercase hexadecimal characters.
    /// An empty input string returns an empty byte array.
    /// </remarks>
    public static byte[] HexToBytes(this string hex)
    {
        Check.NotNull(hex);

        if (hex.Length % 2 == 1)
            throw new ArgumentException("The binary key cannot have an odd number of digits.");

        if (hex.Length == 0)
            return [];

        var len = hex.Length / 2;
        var arr = new byte[len];
        for (var i = 0; i < len; ++i)
        {
            // Nibble is half a byte (0-15, or one hex digit).
            // Low nibble are the bits 0-3; high nibble are bits 4-7.
            var highNibble = GetHexValue(hex[i * 2]);
            var lowNibble = GetHexValue(hex[i * 2 + 1]);
            arr[i] = (byte)((highNibble << 4) + lowNibble);
        }
        return arr;

        static int GetHexValue(char hex)
        {
            return hex switch
            {
                >= 'A' and <= 'F' => hex - 'A' + 10,
                >= 'a' and <= 'f' => hex - 'a' + 10,
                >= '0' and <= '9' => hex - '0',
                _ => throw new ArgumentException($"'{hex}' is not a valid hexadecimal character.", nameof(hex)),
            };
        }
    }

    public static byte[] Base64ToBytes(this string base64, bool autoPad = false)
    {
        if (autoPad == false)
            return Convert.FromBase64String(base64);

        var extraCount = base64.Length % 4;
        if (extraCount == 0)
            return Convert.FromBase64String(base64);

        var padCount = 4 - extraCount;
        using var builder = new ValueStringBuilder(base64.Length + padCount);
        builder.Append(base64);
        builder.Append('=', padCount);
        return Convert.FromBase64String(builder.ToString());
    }

    public static string IfEmpty(this string? str, string defaultValue)
    {
        return str.IsNotEmpty() ? str : defaultValue;
    }

    public static string Replace(this string str, Regex regex, string replacement)
    {
        return regex.Replace(str, replacement);
    }

    public static IEnumerable<string> EnumerateTextElements(this string text)
    {
        for (var en = StringInfo.GetTextElementEnumerator(text); en.MoveNext();)
        {
            yield return en.GetTextElement();
        }
    }

    /// <summary>
    /// Converts all line endings in the string to LF ("\n").
    /// Treats "\r", "\r\n", and "\n" as newlines.
    /// </summary>
    public static string LineEndingToLf(this string text)
    {
        return RegexReplacer.LineEndingToLf.Replace(text);
    }

    /// <summary>
    /// Converts all line endings in the string to CRLF ("\r\n").
    /// Treats "\r", "\r\n", and "\n" as newlines.
    /// </summary>
    public static string LineEndingToCrLf(this string text)
    {
        return RegexReplacer.LineEndingToCrLf.Replace(text);
    }

    /// <summary>
    /// Determines whether the string is enclosed in square brackets [ ].
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string starts with '[' and ends with ']'; otherwise, false.</returns>
    public static bool IsSquareBracketed(this string str)
    {
        return str.Length >= 2 && str.StartsWith('[') && str.EndsWith(']');
    }

    /// <summary>
    /// Removes the leading and trailing square brackets from the string, if present.
    /// </summary>
    /// <param name="str">The string to process.</param>
    /// <returns>The string without surrounding square brackets, or the original string if none are found.</returns>
    public static string TrimSquareBrackets(this string str)
    {
        return str.IsSquareBracketed()
            ? str[1..^1]
            : str;
    }

    extension(string)
    {
        public static string operator *(string str, int count)
        {
            return string.Concat(Enumerable.Repeat(str, count));
        }
    }
}
