using System.IO;

namespace FclEx.Extensions;

public static class ArraySegmentExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty<T>(this ArraySegment<T> source)
    {
        return source.Array.IsNullOrEmpty();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MemoryStream ToMemoryStream(this ArraySegment<byte> source)
    {
        return new MemoryStream(source.Array!, source.Offset, source.Count);
    }
}