namespace FclEx.Extensions;

public static class SpanExtensions
{
    public static string GetString(this Span<byte> span, Encoding? encoding = null)
    {
        return span.AsReadOnlySpan().GetString(encoding);
    }

    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this Span<T> span)
    {
        return span;
    }
}