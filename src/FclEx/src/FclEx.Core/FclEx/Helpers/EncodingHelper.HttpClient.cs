namespace FclEx.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
partial class EncodingHelper
{
    private const int UTF8CodePage = 65001;
    private const int UTF8PreambleLength = 3;
    private const byte UTF8PreambleByte0 = 0xEF;
    private const byte UTF8PreambleByte1 = 0xBB;
    private const byte UTF8PreambleByte2 = 0xBF;
    private const int UTF8PreambleFirst2Bytes = 0xEFBB;

    private const int UTF32CodePage = 12000;
    private const int UTF32PreambleLength = 4;
    private const byte UTF32PreambleByte0 = 0xFF;
    private const byte UTF32PreambleByte1 = 0xFE;
    private const byte UTF32PreambleByte2 = 0x00;
    private const byte UTF32PreambleByte3 = 0x00;
    private const int UTF32OrUnicodePreambleFirst2Bytes = 0xFFFE;

    private const int UnicodeCodePage = 1200;
    private const int UnicodePreambleLength = 2;
    private const byte UnicodePreambleByte0 = 0xFF;
    private const byte UnicodePreambleByte1 = 0xFE;

    private const int BigEndianUnicodeCodePage = 1201;
    private const int BigEndianUnicodePreambleLength = 2;
    private const byte BigEndianUnicodePreambleByte0 = 0xFE;
    private const byte BigEndianUnicodePreambleByte1 = 0xFF;
    private const int BigEndianUnicodePreambleFirst2Bytes = 0xFEFF;

    public static int GetPreambleLength(Span<byte> data, Encoding encoding)
    {
        Check.NotNull(encoding);

        var length = data.Length;

        switch (encoding.CodePage)
        {
            case UTF8CodePage:
                return (length >= UTF8PreambleLength
                        && data[0] == UTF8PreambleByte0
                        && data[1] == UTF8PreambleByte1
                        && data[2] == UTF8PreambleByte2) ? UTF8PreambleLength : 0;
            case UTF32CodePage:
                return (length >= UTF32PreambleLength
                        && data[0] == UTF32PreambleByte0
                        && data[1] == UTF32PreambleByte1
                        && data[2] == UTF32PreambleByte2
                        && data[3] == UTF32PreambleByte3) ? UTF32PreambleLength : 0;
            case UnicodeCodePage:
                return (length >= UnicodePreambleLength
                        && data[0] == UnicodePreambleByte0
                        && data[1] == UnicodePreambleByte1) ? UnicodePreambleLength : 0;

            case BigEndianUnicodeCodePage:
                return (length >= BigEndianUnicodePreambleLength
                        && data[0] == BigEndianUnicodePreambleByte0
                        && data[1] == BigEndianUnicodePreambleByte1) ? BigEndianUnicodePreambleLength : 0;

            default:
                var preamble = encoding.GetPreamble();
                return BufferHasPrefix(data, preamble) ? preamble.Length : 0;
        }
    }

    private static bool BufferHasPrefix(Span<byte> data, byte[]? prefix)
    {
        if (prefix == null || prefix.Length > data.Length || prefix.Length == 0)
            return false;

        for (int i = 0, j = 0; i < prefix.Length; i++, j++)
        {
            if (prefix[i] != data[j])
                return false;
        }

        return true;
    }

    public static bool TryDetectEncoding(Span<byte> data, out Encoding? encoding, out int preambleLength)
    {
        var dataLength = data.Length;

        if (dataLength >= 2)
        {
            var first2Bytes = data[0] << 8 | data[1];

            switch (first2Bytes)
            {
                case UTF8PreambleFirst2Bytes:
                {
                    if (dataLength >= UTF8PreambleLength && data[2] == UTF8PreambleByte2)
                    {
                        encoding = Encoding.UTF8;
                        preambleLength = UTF8PreambleLength;
                        return true;
                    }
                    break;
                }
                case UTF32OrUnicodePreambleFirst2Bytes:
                {
                    // UTF32 not supported on Phone
                    if (dataLength >= UTF32PreambleLength && data[2] == UTF32PreambleByte2 &&
                        data[3] == UTF32PreambleByte3)
                    {
                        encoding = Encoding.UTF32;
                        preambleLength = UTF32PreambleLength;
                    }
                    else
                    {
                        encoding = Encoding.Unicode;
                        preambleLength = UnicodePreambleLength;
                    }
                    return true;
                }
                case BigEndianUnicodePreambleFirst2Bytes:
                {
                    encoding = Encoding.BigEndianUnicode;
                    preambleLength = BigEndianUnicodePreambleLength;
                    return true;
                }
            }
        }

        encoding = null;
        preambleLength = 0;
        return false;
    }
}