namespace FclEx.Comparers;

public unsafe class BytewiseEqualityComparer<T> : IEqualityComparer<T>
{
    public static readonly BytewiseEqualityComparer<T> Instance = new();

    public bool Equals(T? x, T? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result, false))
            return result.Value;

        var p1 = &x;
        var p2 = &y;

        if (p1 == p2)
            return true;

        var span1 = AsSpan(p1);
        var span2 = AsSpan(p2);
#if DEBUG
        var array1 = span1.ToArray();
        var array2 = span2.ToArray();
#endif
        return span1.SequenceEqual(span2);
    }

    public int GetHashCode(T? obj)
    {
        if (obj is null)
            return 0;

        var p = &obj;
        var span = AsSpan(p);
        var hashCode = 0;
        foreach (var m in span)
        {
            hashCode = (hashCode << 3) | (hashCode >> (29)) ^ m;
        }
        return hashCode;
    }

    private static Span<byte> AsSpan(T?* pointer)
    {
        var size = SizeCalculator.SizeOf<T>();

        // 对于引用类型，pointer指向的是目标对象的地址(即二级指针)，
        // 所以还需要将其转换成IntPtr*指针，并最终将指针的内容（也就是目标对象的地址）解析出来。
        // 该地址指向对象的 Method Table。
        // 该地址向前移动一个身位（IntPtr.Size）是实例所在内存片段的首地址，也就是 Object Header 的地址。
        // 该地址向后移动一个身位（IntPtr.Size）是实例的第一个成员变量的地址。
        var (dataSize, dataAddress) = typeof(T).IsValueType switch
        {
            true => (size, new IntPtr(pointer)),
            false => (size - 2 * IntPtr.Size, *(IntPtr*)pointer + IntPtr.Size), 
        };

        var span = new Span<byte>(dataAddress.ToPointer(), dataSize);
        return span;
    }
}