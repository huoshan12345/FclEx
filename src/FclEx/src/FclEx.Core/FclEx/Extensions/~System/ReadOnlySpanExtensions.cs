namespace FclEx.Extensions;

public static class ReadOnlySpanExtensions
{
    public static bool StartsWith<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
    {
        var valueSpan = Span.Create(ref value);
        return span.StartsWith(valueSpan);
    }

    public static string GetString(this ReadOnlySpan<byte> bytes, Encoding? encoding = null)
    {
        return (encoding ?? Encoding.UTF8).GetString(bytes);
    }

#if NET6_0_OR_GREATER
    public static string ToBase64(this ReadOnlySpan<byte> bytes) => Convert.ToBase64String(bytes);
#endif
}