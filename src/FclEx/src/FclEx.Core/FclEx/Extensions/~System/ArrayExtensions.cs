namespace FclEx.Extensions;

public static class ArrayExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOf<T>(this T[] items, T item)
    {
        return Array.IndexOf(items, item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LastIndexOf<T>(this T[] items, T item)
    {
        return Array.LastIndexOf(items, item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear<T>(this T[] items)
    {
        Array.Clear(items, 0, items.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArraySegment<T> ToSegment<T>(this T[]? arr)
    {
        return new(arr ?? []);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArraySegment<T> ToSegment<T>(this T[] arr, int offset, int count)
    {
        return new(arr, offset, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<ArraySegment<T>> Segments<T>(this T[] array, int maxSize)
    {
        return array.ToSegment().Segments(maxSize);
    }

    public static void ForEach<T>(this T[] array, Action<T> action)
    {
        Array.ForEach(array, action);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] array)
    {
        return array.AsSpan();
    }

#if NET6_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SequenceEqual<T>(this T[] bytes, ReadOnlySpan<T> other)
    {
        return bytes.AsReadOnlySpan().SequenceEqual(other);
    }
#endif

}