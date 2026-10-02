namespace FclEx.Extensions;

partial class StringExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static byte[] ToUtf8Bytes(this string input) => input.ToBytes(Encoding.UTF8);

    [MethodImpl(AggressiveInlining)]
    public static byte[] ToBytes(this string input, Encoding? encoding = null) => (encoding ?? Encoding.UTF8).GetBytes(input);

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
}
