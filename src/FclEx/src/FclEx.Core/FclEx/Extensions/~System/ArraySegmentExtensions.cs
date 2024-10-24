namespace FclEx.Extensions;

public static class ArraySegmentExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty<T>(this ArraySegment<T> segment)
    {
        return segment.Array.IsNullOrEmpty() || segment.Count == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MemoryStream ToMemoryStream(this ArraySegment<byte> segment)
    {
        return new MemoryStream(segment.Array!, segment.Offset, segment.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this ArraySegment<T> segment)
    {
        return segment.AsSpan();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArraySegment<T> ToSegment<T>(this ArraySegment<T> segment, int offset, int count)
    {
        Check.NotNull(segment.Array);
        return new(segment.Array, segment.Offset + offset, count);
    }

    public static IEnumerable<ArraySegment<T>> Segments<T>(this ArraySegment<T> segment, int maxSize)
    {
        Check.NotNull(segment.Array);
        Check.GreaterThan(maxSize, 0);

        var count = (segment.Count - 1) / maxSize + 1;
        for (var i = 0; i < count; i++)
        {
            var length = i + 1 == count
                ? segment.Count - i * maxSize
                : maxSize;
            yield return segment.ToSegment(i * maxSize, length);
        }
    }
}