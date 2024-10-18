namespace FclEx.Extensions;

public static class SpanExtensions
{
    public static unsafe string GetString(this ReadOnlySpan<byte> bytes, Encoding? encoding = null)
    {
        if (bytes.IsEmpty)
            return string.Empty;

        encoding ??= Encoding.UTF8;
        fixed (byte* bp = bytes)
        {
            var str = encoding.GetString(bp, bytes.Length);
            return str;
        }
    }

    public static string GetString(this Span<byte> bytes, Encoding? encoding = null)
    {
        return ((ReadOnlySpan<byte>)bytes).GetString(encoding);
    }
}