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
            throw new ArgumentException($"The type {type.LongName()} is not {predicateName} due to: " + ex?.Message, nameof(type), ex);
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
        (var flag, ex) = type.Check(nameof(IsBlittable), CheckBlittable);
        return flag;
    }

    public static bool IsMarshalable(this Type type, [NotNullWhen(false)] out Exception? ex)
    {
        (var flag, ex) = type.Check(nameof(IsMarshalable), m => CheckMarshalable(m, null, null));
        return flag;
    }

    private static (bool, Exception?) Check(this Type type, string name, Action<Type> action)
    {
        return _flagCache.GetOrAdd((type, name), m =>
        {
            try
            {
                action(type);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        });
    }

    private static void CheckBlittable(this Type type)
    {
        // we use Pinned-GCHandle to check if blittable, but there are some corner cases requiring special handling.
        // * Char and Boolean are not pinnable, but array of these types are pinnable.
        //   So we need to check if element type is pinnable as well.
        // * All generic types are not blittable, but ValueTuple<T> is pinnable. 
        // * Nullable of blittable types are pinnable, but they are not blittable.

        if (type == typeof(string)
            || type == typeof(object)
            || type.IsAssignableTo(typeof(Delegate)))
            Throw(null);

        // Exclude all generic types as well as nullable types. 
        if (type.IsGenericType)
            Throw("generic");

        if (type.IsAbstract)
            Throw("abstract");

        object instance;
        if (type.GetElementType() is { IsArray: false } elementType && type.GetArrayRank() == 1)
        {
            CheckBlittable(elementType); // check if element type is pinnable as well.

            var array = Array.CreateInstance(elementType, 1);
            var entry = ObjectHelper.GetUninitializedObject(elementType);
            array.SetValue(entry, 0);
            instance = array;
        }
        else
        {
            instance = ObjectHelper.GetUninitializedObject(type);
        }

        // NOTE: 
        GCHandle.Alloc(instance, GCHandleType.Pinned).Free();
        return;

        [DoesNotReturn]
        void Throw(string? reason)
        {
            var reasonSuffix = reason is null
                ? string.Empty
                : $" because it is {reason}";
            var error = $"The type '{type.LongName()}' is not blittable{reasonSuffix}.";
            throw new ArgumentException(error, nameof(type));
        }
    }

    private static void CheckMarshalable(Type type, FieldInfo? field, HashSet<Type>? visited)
    {
        type = type.UnwrapNullable();

        if (type.IsGenericType)
            Throw("generic");

        if (type.IsAbstract)
            Throw("abstract");

        if (type.IsEnum
            || type == typeof(char)
            || type == typeof(bool)
            || Types.BlittableTypes.Contains(type))
            return;

        if (field is not null && field.IsDefined(typeof(MarshalAsAttribute), false))
            return;

        if (type.IsAutoLayout)
            Throw("auto layout");

        if (type == typeof(string)
            || type == typeof(object)
            || type.IsAssignableTo(typeof(Delegate)))
            Throw(null);

        _ = Marshal.SizeOf(type);

        visited ??= [];

        if (visited.Add(type) == false)
            Throw("circular referenced");

        foreach (var m in type.GetAllInstanceFields())
        {
            CheckMarshalable(m.FieldType, m, visited);
        }

        return;

        [DoesNotReturn]
        void Throw(string? reason)
        {
            var reasonSuffix = reason is null
                ? string.Empty
                : $" because it is {reason}";
            var error = $"The type '{type.LongName()}' is not marshalable{reasonSuffix}.";
            throw new ArgumentException(error, nameof(type));
        }
    }
}