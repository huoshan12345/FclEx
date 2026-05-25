namespace FclEx.Extensions;

public static class EncodingExtensions
{
    private static readonly Encoding _utf8WithoutBom = new UTF8Encoding(false);

    extension(Encoding)
    {
        /// <summary>
        /// Gets a UTF-8 encoding instance that does not emit a byte order mark.
        /// </summary>
        public static Encoding Utf8WithoutBom => _utf8WithoutBom;
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
