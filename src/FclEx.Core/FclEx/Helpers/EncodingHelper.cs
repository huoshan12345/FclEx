namespace FclEx.Helpers;

public static partial class EncodingHelper
{
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
        return GetEncoding(fs, defaultEncoding);
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

    private static readonly Encoding[] BomEncodings =
    [
        new UTF32Encoding(bigEndian: true, byteOrderMark: true),
        Encoding.UTF32,
        Encoding.BigEndianUnicode,
        Encoding.Unicode,
        Encoding.UTF8,
    ];
}
