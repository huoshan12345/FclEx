namespace FclEx.Extensions;

partial class BytesExtensions
{
    public static byte[] Base32ToBytes(this string input)
    {
        if (input.IsNullOrEmpty())
            return [];

        input = input.TrimEnd('='); //remove padding characters
        var byteCount = input.Length * 5 / 8; //this must be TRUNCATED
        var returnArray = new byte[byteCount];

        byte curByte = 0, bitsRemaining = 8;
        var arrayIndex = 0;

        foreach (var c in input)
        {
            var cValue = CharToValue(c);

            var mask = 0;
            if (bitsRemaining > 5)
            {
                mask = cValue << (bitsRemaining - 5);
                curByte = (byte)(curByte | mask);
                bitsRemaining -= 5;
            }
            else
            {
                mask = cValue >> (5 - bitsRemaining);
                curByte = (byte)(curByte | mask);
                returnArray[arrayIndex++] = curByte;
                curByte = (byte)(cValue << (3 + bitsRemaining));
                bitsRemaining += 3;
            }
        }

        //if we didn't end with a full byte
        if (arrayIndex != byteCount)
        {
            returnArray[arrayIndex] = curByte;
        }

        return returnArray;
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

    private static int CharToValue(char c)
    {
        var value = (int)c;

        return value switch
        {
            < 91 and > 64 => value - 65, // 65-90 == uppercase letters
            < 56 and > 49 => value - 24, // 50-55 == numbers 2-7
            < 123 and > 96 => value - 97, // 97-122 == lowercase letters
            _ => throw new ArgumentException("Character is not a Base32 character.", nameof(c))
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