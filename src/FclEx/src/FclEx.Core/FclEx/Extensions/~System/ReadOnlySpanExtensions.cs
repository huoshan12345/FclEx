namespace FclEx.Extensions;

public static class ReadOnlySpanExtensions
{
    public static bool StartsWith<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
    {
        var valueSpan = Span.Create(ref value);
        return span.StartsWith(valueSpan);
    }
}