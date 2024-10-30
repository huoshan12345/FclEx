namespace FclEx.Extensions;

partial class TypeExtensions
{
    private delegate bool TypePredicate(Type type, [NotNullWhen(false)] out Exception? ex);

    private static readonly ConcurrentDictionary<(Type, string), (bool, Exception?)> _flagCache = new();

    private static void Ensure(this Type type, TypePredicate predicate, string predicateName)
    {
        if (predicate(type, out var ex))
            return;

        if (ex is ArgumentException { ParamName: nameof(type) })
        {
            ex.ReThrow();
        }
        else
        {
            throw new ArgumentException($"The type {type.LongName()} is not {predicateName} due to: " + ex.Message, nameof(type), ex);
        }
    }

    public static void EnsureBlittable(this Type type)
    {
        type.Ensure(IsBlittable, "blittable");
    }

    public static void EnsureMarshalable(this Type type)
    {
        type.Ensure(IsMarshalable, "marshalable");
    }

    /// <summary>
    /// Blittable types have an identical presentation in memory for both managed and unmanaged code.<br/>
    /// 如果某个参数为Blittable类型，在一个P/Invoke方法调用非托管方法的时候，该参数就无需要作任何的转换。<br/>
    /// 与之类似，如果调用方法的返回值是Blittable类型，在回到托管世界后也无需转换。<br/>
    /// <br/>
    /// 如下的类型属于Blittable类型范畴：<br/>
    /// * 除 <see cref="bool"/> 和 <see cref="char"/> 之外的12种基元类型，因为布尔值True在不同的平台可能会表示成1或者-1, 对应的字节数可能是1、2或者4，字符涉及不同的编码（Unicode和ANSI），所以这两种类型并非Blittable类型。<br/>
    /// * Blittable基元类型的一维数组。但如果包含这种数组作为成员的类型不是Blittable，因为其持有的是数组的引用而不是其内容本身.<br/>
    /// * 采用Sequential和Explicitly布局的且只包含Blittable类型成员的结构或者类,因为采用这两种布局的对象最终会按照一种确定的格式转换成对应的C风格的结构体。
    /// 如果采用Auto布局，CLR会按照少占用内存的原则对字段成员重新排序，意味着其内存结构是不确定的。<br/>
    /// <br/>
    /// 注意：<see cref="DateTime"/> / <see cref="DateTimeOffset"/> 都采用Auto布局.
    /// <see cref="Guid"/> 虽然是一个默认采用Sequential布局的结构体，但是最终映射在内存种的字节依赖于字节序（Endianness），所以具有这三种类型字段的结构体或者类都不是Blittable类型。 
    /// </summary>
    public static bool IsBlittable(this Type type, [NotNullWhen(false)] out Exception? ex)
    {
        (var flag, ex) = type.GetFlag(nameof(IsBlittable), IsBlittableImpl);
        return flag;
    }

    public static bool IsMarshalable(this Type type, [NotNullWhen(false)] out Exception? ex)
    {
        (var flag, ex) = type.GetFlag(nameof(IsMarshalable), IsMarshalableImpl);
        return flag;
    }

    private static (bool, Exception?) GetFlag(this Type type, string name, Func<Type, bool> function)
    {
        return _flagCache.GetOrAdd((type, name), m =>
        {
            try
            {
                var flag = function(type);
                return (flag, null);
            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        });
    }

    private static bool IsBlittableImpl(this Type type)
    {
        object instance;
        if (type.GetElementType() is { } elementType)
        {
            var array = Array.CreateInstance(elementType, 1);
            var entry = ObjectHelper.GetUninitializedObject(elementType);
            array.SetValue(entry, 0);
            instance = array;
        }
        else
        {
            instance = ObjectHelper.GetUninitializedObject(type);
        }

        GCHandle.Alloc(instance, GCHandleType.Pinned).Free();
        return true;
    }

    private static bool IsMarshalableImpl(this Type type)
    {
        if (type.IsBlittable(out _))
            return true;

        type = type.UnwrapNullable();

        if (type.IsGenericType)
        {
            throw new ArgumentException($"The type {type.LongName()} is not marshalable because it is generic.", nameof(type));
        }

        if (type.IsAbstract)
        {
            throw new ArgumentException($"The type {type.LongName()} is not marshalable because it is abstract.", nameof(type));
        }

        // You cannot use the GetCustomAttributes method to determine whether the StructLayoutAttribute has been applied to a type.
        if (type.IsAutoLayout)
        {
            throw new ArgumentException($"The type {type.LongName()} is not marshalable because it is auto-layout.", nameof(type));
        }

        if (type.IsLayoutSequential || type.IsExplicitLayout)
        {
            return true;
        }

        if (type.GetCustomAttribute<MarshalAsAttribute>() is not null)
        {
            return true;
        }


        return false;
    }
}