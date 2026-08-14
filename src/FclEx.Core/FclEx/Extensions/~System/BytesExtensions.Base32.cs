namespace FclEx.Extensions;

partial class BytesExtensions
{
    public static byte[] Base32ToBytes(this string input)
    {
        Check.NotNull(input);
        if (input.Length == 0)
            return [];

        var paddingIndex = input.IndexOf('=');
        var dataLength = paddingIndex < 0 ? input.Length : paddingIndex;
        var paddingLength = input.Length - dataLength;
        if (paddingIndex >= 0)
        {
            if (input.Length % 8 != 0)
                throw new ArgumentException("Base32 padding must appear only at the end of a complete 8-character block.", nameof(input));

            for (var i = paddingIndex; i < input.Length; i++)
            {
                if (input[i] != '=')
                    throw new ArgumentException("Base32 padding must appear only at the end of a complete 8-character block.", nameof(input));
            }
        }

        var remainder = dataLength % 8;
        var expectedPaddingLength = remainder switch
        {
            0 => 0,
            2 => 6,
            4 => 4,
            5 => 3,
            7 => 1,
            _ => throw new ArgumentException("The Base32 input has an invalid length.", nameof(input)),
        };

        if (paddingLength > 0 && paddingLength != expectedPaddingLength)
            throw new ArgumentException("The Base32 input has an invalid padding length.", nameof(input));

        var result = new byte[dataLength * 5 / 8];
        var resultIndex = 0;
        var bitBuffer = 0;
        var bitCount = 0;

        for (var i = 0; i < dataLength; i++)
        {
            bitBuffer = (bitBuffer << 5) | CharToValue(input[i], nameof(input));
            bitCount += 5;
            if (bitCount < 8)
                continue;

            bitCount -= 8;
            result[resultIndex++] = (byte)(bitBuffer >> bitCount);
            bitBuffer &= (1 << bitCount) - 1;
        }

        if (bitBuffer != 0)
            throw new ArgumentException("The unused bits at the end of the Base32 input must be zero.", nameof(input));

        return result;
    }

    public static string ToBase32(this byte[] bytes)
    {
        return bytes.AsReadOnlySpan().ToBase32();
    }

    public static string ToBase32(this ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
            return string.Empty;

        var charCount = (int)Math.Ceiling(bytes.Length / 5d) * 8;
        var returnArray = new char[charCount];

        byte nextChar = 0, bitsRemaining = 5;
        var arrayIndex = 0;

        foreach (var b in bytes)
        {
            nextChar = (byte)(nextChar | (b >> (8 - bitsRemaining)));
            returnArray[arrayIndex++] = ValueToChar(nextChar);

            if (bitsRemaining < 4)
            {
                nextChar = (byte)((b >> (3 - bitsRemaining)) & 31);
                returnArray[arrayIndex++] = ValueToChar(nextChar);
                bitsRemaining += 5;
            }

            bitsRemaining -= 3;
            nextChar = (byte)((b << bitsRemaining) & 31);
        }

        //if we didn't end with a full char
        // ReSharper disable once InvertIf
        if (arrayIndex != charCount)
        {
            returnArray[arrayIndex++] = ValueToChar(nextChar);
            while (arrayIndex != charCount) returnArray[arrayIndex++] = '='; //padding
        }

        return new string(returnArray);
    }

    private static int CharToValue(char c, string parameterName)
    {
        var value = (int)c;

        return value switch
        {
            < 91 and > 64 => value - 65, // 65-90 == uppercase letters
            < 56 and > 49 => value - 24, // 50-55 == numbers 2-7
            < 123 and > 96 => value - 97, // 97-122 == lowercase letters
            _ => throw new ArgumentException($"'{c}' is not a Base32 character.", parameterName)
        };
    }

    private static char ValueToChar(byte b)
    {
        return b switch
        {
            < 26 => (char)(b + 65),
            < 32 => (char)(b + 24),
            _ => throw new ArgumentException("Byte is not a value Base32 value.", nameof(b))
        };
    }
}
