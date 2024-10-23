namespace FclEx.Extensions;

public static class EncodingExtensions
{
#if NETSTANDARD2_0
    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        fixed (byte* p = bytes)
        {
            return encoding.GetString(p, bytes.Length);
        }
    }
#endif
}