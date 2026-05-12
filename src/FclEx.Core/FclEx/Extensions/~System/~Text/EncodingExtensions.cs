namespace FclEx.Extensions;

public static class EncodingExtensions
{
#if !NET5_0_OR_GREATER
    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        fixed (byte* p = bytes)
        {
            return encoding.GetString(p, bytes.Length);
        }
    }
#endif
}