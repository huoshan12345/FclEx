namespace FclEx.Extensions;

public static class SpanExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static string GetString(this Span<byte> span, Encoding? encoding = null)
    {
        return span.AsReadOnlySpan().GetString(encoding);
    }

    [MethodImpl(AggressiveInlining)]
    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this Span<T> span)
    {
        return span;
    }

    /// <summary>
    /// Casts a Span of one primitive type <typeparamref name="TFrom"/> to another primitive type <typeparamref name="TTo"/>.
    /// These types may not contain pointers or references. This is checked at runtime in order to preserve type safety.
    /// </summary>
    /// <remarks>
    /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
    /// </remarks>
    /// <param name="span">The source slice, of type <typeparamref name="TFrom"/>.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> contains pointers.
    /// </exception>
    [MethodImpl(AggressiveInlining)]
    public static Span<TTo> Cast<TFrom, TTo>(this Span<TFrom> span)
        where TFrom : struct
        where TTo : struct
    {
        return MemoryMarshal.Cast<TFrom, TTo>(span);
    }
}