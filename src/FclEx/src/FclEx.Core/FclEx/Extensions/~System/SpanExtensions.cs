namespace FclEx.Extensions;

public static class SpanExtensions
{
    public static string GetString(this Span<byte> bytes, Encoding? encoding = null)
    {
        return ((ReadOnlySpan<byte>)bytes).GetString(encoding);
    }
}