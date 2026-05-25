namespace FclEx.Extensions;

public static class ArrayExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static int IndexOf<T>(this T[] items, T item)
    {
        return Array.IndexOf(items, item);
    }

    [MethodImpl(AggressiveInlining)]
    public static int LastIndexOf<T>(this T[] items, T item)
    {
        return Array.LastIndexOf(items, item);
    }

    [MethodImpl(AggressiveInlining)]
    public static void Clear<T>(this T[] items)
    {
        Array.Clear(items, 0, items.Length);
    }

    [MethodImpl(AggressiveInlining)]
    public static ArraySegment<T> ToSegment<T>(this T[]? arr)
    {
        return new(arr ?? []);
    }

    [MethodImpl(AggressiveInlining)]
    public static ArraySegment<T> ToSegment<T>(this T[] arr, int offset, int count)
    {
        return new(arr, offset, count);
    }

    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<ArraySegment<T>> Segments<T>(this T[]? array, int maxSize)
    {
        return array.ToSegment().Segments(maxSize);
    }

    public static void ForEach<T>(this T[] array, Action<T> action)
    {
        Array.ForEach(array, action);
    }

    [MethodImpl(AggressiveInlining)]
    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] array)
    {
        return array.AsSpan();
    }

    [MethodImpl(AggressiveInlining)]
    public static int FindIndex<T>(this T[] array, Predicate<T> match)
    {
        return Array.FindIndex(array, match);
    }

    [MethodImpl(AggressiveInlining)]
    public static int FindIndex<T>(this T[] array, int startIndex, Predicate<T> match)
    {
        return Array.FindIndex(array, startIndex, match);
    }

    [MethodImpl(AggressiveInlining)]
    public static int FindIndex<T>(this T[] array, int startIndex, int count, Predicate<T> match)
    {
        return Array.FindIndex(array, startIndex, count, match);
    }

    [MethodImpl(AggressiveInlining)]
    public static int FindLastIndex<T>(this T[] array, Predicate<T> match)
    {
        return Array.FindLastIndex(array, match);
    }

    [MethodImpl(AggressiveInlining)]
    public static int FindLastIndex<T>(this T[] array, int startIndex, Predicate<T> match)
    {
        return Array.FindLastIndex(array, startIndex, match);
    }

    [MethodImpl(AggressiveInlining)]
    public static int FindLastIndex<T>(this T[] array, int startIndex, int count, Predicate<T> match)
    {
        return Array.FindLastIndex(array, startIndex, count, match);
    }

#if NET6_0_OR_GREATER
    [MethodImpl(AggressiveInlining)]
    public static bool SequenceEqual<T>(this T[] bytes, ReadOnlySpan<T> other)
    {
        return bytes.AsReadOnlySpan().SequenceEqual(other);
    }
#endif

    [MethodImpl(AggressiveInlining)]
    public static T? FindLast<T>(this T[] array, Predicate<T> predicate)
    {
        return Array.FindLast(array, predicate);
    }

    extension<T>(T[])
    {
        public static T[] operator +(T[] array, T[] other)
        {
            var result = new T[array.Length + other.Length];
            array.CopyTo(result, 0);
            other.CopyTo(result, array.Length);
            return result;
        }
    }

    extension(Array)
    {
#if !NET5_0_OR_GREATER
        public static int MaxLength => 2147483591;
#endif
    }
}