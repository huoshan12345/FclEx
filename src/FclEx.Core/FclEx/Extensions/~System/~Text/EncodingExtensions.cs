namespace FclEx.Extensions;

public static class EncodingExtensions
{
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