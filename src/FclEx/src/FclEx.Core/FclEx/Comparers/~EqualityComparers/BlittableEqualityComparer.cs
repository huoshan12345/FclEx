namespace FclEx.Comparers;

/// <summary>
/// Blittable类型要求在托管内存和非托管内存具有完全一致的表示。<br/>
/// 如果某个参数为Blittable类型，在一个P/Invoke方法调用非托管方法的时候，该参数就无需要作任何的转换。<br/>
/// 与之类似，如果调用方法的返回值是Blittable类型，在回到托管世界后也无需转换。<br/>
/// <br/>
/// 如下的类型属于Blittable类型范畴：<br/>
/// * 除Boolean(bool)和Char（char）之外的12种基元类型，因为布尔值True在不同的平台可能会表示成1或者-1, 对应的字节数可能是1、2或者4，字符涉及不同的编码（Unicode和ANSI），所以这两种类型并非Blittable类型.<br/>
/// * Blittable基元类型的一维数组.<br/>
/// * 采用Sequential和Explicitly布局的且只包含Blittable类型成员的结构或者类,因为采用这两种布局的对象最终会按照一种确定的格式转换成对应的C风格的结构体。
/// 如果采用Auto布局，CLR会按照少占用内存的原则对字段成员重新排序，意味着其内存结构是不确定的。<br/>
/// <br/>
/// 顺便强调一下，<see cref="DateTime"/> / <see cref="DateTimeOffset"/> 都采用Auto布局.
/// <see cref="Guid"/> 虽然是一个默认采用Sequential布局的结构体，但是最终映射在内存种的字节依赖于字节序（Endianness），
/// 所以具有这三种类型字段的结构体或者类都不是Blittable类型。
/// </summary>
/// <typeparam name="T"></typeparam>
public class BlittableEqualityComparer<T> : IEqualityComparer<T>
{
    private static readonly bool IsBlittableType = IsBlittable(typeof(T));

    public static readonly BlittableEqualityComparer<T> Instance = new();

    public bool Equals(T? x, T? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        var bytes1 = x.BlittableToBytes();
        var bytes2 = y.BlittableToBytes();
        return bytes1.SequenceEqual(bytes2);
    }

    public int GetHashCode(T? obj)
    {
        if (obj is null)
            return 0;

        var bytes = obj.BlittableToBytes();
        return bytes.ComputeHashCode();
    }

    private static bool IsBlittable(Type type)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            return elementType.IsValueType && IsBlittable(elementType);
        }

        // exception will be raised if type is not blittable.
        var instance = ObjectHelper.GetUninitializedObject(type);
        GCHandle.Alloc(instance, GCHandleType.Pinned).Free();
        return true;
    }
}