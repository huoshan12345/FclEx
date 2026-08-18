namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class EncodingExtensions
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

    private static readonly Encoding[] BomEncodings =
    [
        new UTF32Encoding(bigEndian: true, byteOrderMark: true),
        Encoding.UTF32,
        Encoding.BigEndianUnicode,
        Encoding.Unicode,
        Encoding.UTF8,
    ];

    private static readonly Encoding _utf8WithoutBom = new UTF8Encoding(false);

    extension(Encoding)
    {
        /// <summary>
        /// Gets a UTF-8 encoding instance that does not emit a byte order mark.
        /// </summary>
        public static Encoding Utf8WithoutBom => _utf8WithoutBom;

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
        
        public static Encoding DetectEncoding(string filePath, Encoding? defaultEncoding = null)
        {
            defaultEncoding ??= Encoding.UTF8;

            using var reader = new StreamReader(filePath, defaultEncoding, true);
            reader.Peek();
            return reader.CurrentEncoding;
        }

        public static Encoding GetEncoding(string filePath, Encoding? defaultEncoding = null)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return Encoding.GetEncoding(fs, defaultEncoding);
        }

        /// <summary>
        /// Detects an encoding from a byte-order mark at the beginning of a stream.
        /// </summary>
        /// <param name="stream">A readable, seekable stream to inspect.</param>
        /// <param name="defaultEncoding">The encoding returned when the stream has no recognized byte-order mark.</param>
        /// <returns>The encoding identified by the byte-order mark, or <paramref name="defaultEncoding"/> when no byte-order mark is present.</returns>
        /// <exception cref="ArgumentException"><paramref name="stream"/> is not readable or seekable.</exception>
        /// <remarks>
        /// The stream remains open and its original position is restored before this method returns.
        /// This method intentionally does not infer UTF-8 from the content when no byte-order mark is present.
        /// </remarks>
        public static Encoding GetEncoding(Stream stream, Encoding? defaultEncoding = null)
        {
            Check.NotNull(stream);
            defaultEncoding ??= Encoding.UTF8;

            if (stream.CanRead == false)
                throw new ArgumentException("The stream must be readable.", nameof(stream));

            if (stream.CanSeek == false)
                throw new ArgumentException("The stream must support seeking.", nameof(stream));

            var originalPosition = stream.Position;
            try
            {
                stream.Position = 0;
                var buffer = new byte[4];
                var count = 0;
                while (count < buffer.Length)
                {
                    var read = stream.Read(buffer, count, buffer.Length - count);
                    if (read == 0)
                        break;

                    count += read;
                }

                foreach (var encoding in BomEncodings)
                {
                    var preamble = encoding.GetPreamble();
                    if (count >= preamble.Length && buffer.AsSpan(0, preamble.Length).SequenceEqual(preamble))
                        return encoding;
                }

                return defaultEncoding;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }
    }

    public static int GetPreambleLength(this Encoding encoding, Span<byte> data)
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

#if !NET5_0_OR_GREATER
    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            return "";

        fixed (byte* p = bytes)
        {
            // in nfx, a pointer to empty byte array will cause null reference exception in GetString
            return encoding.GetString(p, bytes.Length);
        }
    }
#endif
}
