namespace FclEx.Comparers;

public unsafe class BytewiseEqualityComparer<T> : IEqualityComparer<T>
{
    public static readonly BytewiseEqualityComparer<T> Instance = new();

    public bool Equals(T? x, T? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result, false))
            return result.Value;

        var span1 = AsSpan(x);
        var span2 = AsSpan(y);
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

        var span = AsSpan(obj);
        var hashCode = 0;
        foreach (var m in span)
        {
            hashCode = (hashCode << 3) | (hashCode >> (29)) ^ m;
        }
        return hashCode;
    }

    private static Span<byte> AsSpan(T obj)
    {
        var size = SizeCalculator.SizeOf<T>();
        var pointer = Unsafe.AsPointer(ref obj);
        var pointer2 = &obj;

        // 对于引用类型，pointer指向的是目标对象的地址(即二级指针)，
        // 所以还需要将其转换成IntPtr*指针，并最终将指针的内容（也就是目标对象的地址）解析出来。
        // 由于变量指向的地址并非目标实例映射内存字节的首地址，仅仅是存储方法表地址的地方，
        // 所以还需要向前移动一个身位（IntPtr.Size）才是实例所在内存片段的首地址。即 Object Header 的地址。
        var head = typeof(T).IsValueType
            ? new IntPtr(pointer)
            : *(IntPtr*)pointer - IntPtr.Size;

        var span = new Span<byte>(head.ToPointer(), size);
        return span;
    }
}