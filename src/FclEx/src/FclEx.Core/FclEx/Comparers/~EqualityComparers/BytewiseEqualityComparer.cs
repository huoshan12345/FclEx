namespace FclEx.Comparers;

public unsafe class BytewiseEqualityComparer<T> : IEqualityComparer<T> where T : struct
{
    public static readonly BytewiseEqualityComparer<T> Instance = new();

    private static readonly int MarshalSize = Marshal.SizeOf<T>();
    private static readonly int Size = sizeof(T);
    private static readonly int Offset = Size - MarshalSize;

    public bool Equals(T x, T y)
    {
        var p1 = ((byte*)&x) + Offset;
        var p2 = ((byte*)&y) + Offset;
        var span1 = new Span<byte>(p1, MarshalSize);
        var span2 = new Span<byte>(p2, MarshalSize);
        return span1.SequenceEqual(span2);
    }

    public int GetHashCode(T obj)
    {
        var p = ((byte*)&obj) + Offset;
        var span = new Span<byte>(p, MarshalSize);
        var hashCode = 0;
        foreach (var m in span)
        {
            hashCode = (hashCode << 3) | (hashCode >> (29)) ^ m;
        }
        return hashCode;
    }
}