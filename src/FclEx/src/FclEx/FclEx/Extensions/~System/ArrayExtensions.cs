using System.Collections.Generic;

namespace FclEx.Extensions;

public static class ArrayExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOf<T>(this T[]? items, T item)
    {
        return items != null ? Array.IndexOf(items, item) : -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LastIndexOf<T>(this T[]? items, T item)
    {
        return items != null ? Array.LastIndexOf(items, item) : -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear<T>(this T[] items)
    {
        Array.Clear(items, 0, items.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArraySegment<T> ToSegmentOrEmpty<T>(this T[]? arr)
    {
        return new(arr ?? Array.Empty<T>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArraySegment<T> ToSegment<T>(this T[] arr)
    {
        return new(arr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArraySegment<T> ToSegment<T>(this T[] arr, int offset, int count)
    {
        return new(arr, offset, count);
    }

    public static IEnumerable<ArraySegment<T>> Segments<T>(this T[] array, int maxSize)
    {
        Check.NotNull(array);
        Check.GreaterThan(maxSize, 0);

        var count = (array.Length - 1) / maxSize + 1;
        for (var i = 0; i < count; i++)
        {
            var length = i + 1 == count ? array.Length - i * maxSize : maxSize;
            yield return array.ToSegment(i * maxSize, length);
        }
    }

    public static void ForEach<T>(this T[] array, Action<T> action)
    {
        Array.ForEach(array, action);
    }
}