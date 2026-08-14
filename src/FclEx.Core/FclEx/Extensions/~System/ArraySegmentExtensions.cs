namespace FclEx.Extensions;

public static class ArraySegmentExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static bool IsNullOrEmpty<T>(this ArraySegment<T> segment)
    {
        return segment.Array.IsNullOrEmpty() || segment.Count == 0;
    }

    [MethodImpl(AggressiveInlining)]
    public static MemoryStream ToMemoryStream(this ArraySegment<byte> segment)
    {
        return new MemoryStream(segment.Array!, segment.Offset, segment.Count);
    }

    [MethodImpl(AggressiveInlining)]
    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this ArraySegment<T> segment)
    {
        return segment.AsSpan();
    }

#if !NET5_0_OR_GREATER
    [MethodImpl(AggressiveInlining)]
    public static ArraySegment<T> Slice<T>(this ArraySegment<T> segment, int offset, int count)
    {
        Check.NotNull(segment.Array);

        if ((uint)offset > (uint)segment.Count)
            throw new ArgumentOutOfRangeException(nameof(offset));

        if ((uint)count > (uint)(segment.Count - offset))
            throw new ArgumentOutOfRangeException(nameof(count));

        return new(segment.Array, segment.Offset + offset, count);
    }
#endif

    public static IEnumerable<ArraySegment<T>> Segments<T>(this ArraySegment<T> segment, int maxSize)
    {
        Check.NotNull(segment.Array);
        Check.GreaterThan(maxSize, 0);

        if (segment.Count == 0)
            yield break;

        var count = (segment.Count - 1) / maxSize + 1;
        for (var i = 0; i < count; i++)
        {
            var length = i + 1 == count
                ? segment.Count - i * maxSize
                : maxSize;
            yield return segment.Slice(i * maxSize, length);
        }
    }
}
