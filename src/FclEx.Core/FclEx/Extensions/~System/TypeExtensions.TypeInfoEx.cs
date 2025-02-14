namespace FclEx.Extensions;

partial class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, TypeInfoEx> TypeInfoDic = new();

#if NETSTANDARD2_0
    private static readonly Lazy<PropertyInfo?> _isByRefLike = new(() => typeof(Type).GetProperty("IsByRefLike", BindingAttributes.AllDeclared));
#endif

    public static TypeInfoEx GetTypeInfoEx(this Type type)
    {
        FclEx.Check.NotNull(type);
        return TypeInfoDic.GetOrAdd(type, GetTypeInfoExtInternal);

        static TypeInfoEx GetTypeInfoExtInternal(Type type)
        {
            var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            var defaultValue = GetDefaultValueInternal(type, nullableUnderlyingType);
            var enumerableUnderlyingType = GetEnumerableUnderlyingTypeInternal(type);
            var simpleName = SimpleNameInternal(type);
            var shortName = ShortNameInternal(type, simpleName);
            var longName = LongNameInternal(type, shortName);
            var isInteger = IsIntegerInternal(type, nullableUnderlyingType);
            var isFloat = IsFloatInternal(type, nullableUnderlyingType);

            return new TypeInfoEx(
                Type: type,
                NullableUnderlyingType: nullableUnderlyingType,
                EnumerableUnderlyingType: enumerableUnderlyingType,
                DefaultValue: defaultValue,
                SimpleName: simpleName,
                ShortName: shortName,
                LongName: longName,
                IsInteger: isInteger,
                IsFloat: isFloat);
        }

        static object? GetDefaultValueInternal(Type type, Type? nullableUnderlyingType)
        {
            /*
                Acc_CreateGeneric = Cannot create a type for which Type.ContainsGenericParameters is true.
                Acc_CreateAbst = Cannot create an abstract class.
                Acc_CreateInterface = Cannot create an instance of an interface.
                Acc_NotClassInit = Type initializer was not callable.
                Acc_CreateGenericEx = Cannot create an instance of {0} because Type.ContainsGenericParameters is true.
                Acc_CreateArgIterator = Cannot dynamically create an instance of ArgIterator.
                Acc_CreateAbstEx = Cannot create an instance of {0} because it is an abstract class.
                Acc_CreateInterfaceEx = Cannot create an instance of {0} because it is an interface.
                Acc_CreateVoid = Cannot dynamically create an instance of System.Void.
                Acc_ReadOnly = Cannot set a constant field.
                Acc_RvaStatic = SkipVerification permission is needed to modify an image-based (RVA) static field.
                Access_Void = Cannot create an instance of void.
                Cannot create boxed ByRef-like values.
            */

            if (type.IsValueType == false
                || nullableUnderlyingType != null
                || type.ContainsGenericParameters
                || type.Name == "ArgIterator"
                // Byref-like structures are declared using ref struct keyword in C#. 
#if NETSTANDARD2_0
                || IsByRefLike(type)
#else
                || type.IsByRefLike
#endif
                || type == typeof(void))
                return null;

            return Activator.CreateInstance(type);
        }
#if NETSTANDARD2_0
        static bool IsByRefLike(Type type)
        {
            return _isByRefLike.Value is { } field && field.GetValue<bool>(type);
        }
#endif
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        static Type? GetEnumerableUnderlyingTypeInternal(Type type)
        {
            // type is Array
            if (type.IsArray)
                return type.GetElementType();

            // type is IEnumerable<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GenericTypeArguments.FirstOrDefault() ?? type.GetTypeInfo().GenericTypeParameters.FirstOrDefault();

            // type implements IEnumerable<T>
            if (type.GetImplementedInterface(typeof(IEnumerable<>)) is { } iEnumerableType)
                return iEnumerableType.GenericTypeArguments[0];

            // type implements IEnumerable
            if (type.IsAssignableTo(typeof(IEnumerable)))
                return typeof(object);

            return null;
        }

        static string SimpleNameInternal(Type type)
        {
            var name = type.Name;

            if (type.IsGenericType == false)
                return name;

            var index = name.IndexOf('`');
            return index == -1 ? name : name[..index];
        }

        static string ShortNameInternal(Type type, string simpleName)
        {
            if (!type.IsGenericType) return type.Name;
            var paraName = string.Join(", ", type.GenericTypeArguments!.Select(m => m.ShortName()));
            return simpleName + "<" + paraName + ">";
        }

        static string LongNameInternal(Type type, string shortName)
        {
            return GetTypePrefixInternal(type) + shortName;
        }

        static string GetTypePrefixInternal(Type type)
        {
            if (type.IsNested)
            {
                var declaringType = type.DeclaringType!;
                return GetTypePrefixInternal(declaringType) + declaringType.ShortName() + ".";
            }
            else
            {
                if (type.IsGenericParameter)
                {
                    var declaringType = type.DeclaringType!;
                    if (type.DeclaringMethod != null)
                    {
                        return declaringType.LongName()
                               + "." + type.DeclaringMethod.Name
                               + ".";
                    }
                    else
                    {
                        return declaringType.LongName() + ".";
                    }
                }
                if (type.Namespace == null)
                {
                    return "global::";
                }
                else
                {
                    return type.Namespace + ".";
                }
            }
        }

        static bool IsIntegerInternal(Type type, Type? nullableUnderlyingType)
        {
            type = nullableUnderlyingType ?? type;
            return type == typeof(long)
                   || type == typeof(ulong)
                   || type == typeof(int)
                   || type == typeof(uint)
                   || type == typeof(short)
                   || type == typeof(ushort)
                   || type == typeof(byte)
                   || type == typeof(sbyte);
        }

        static bool IsFloatInternal(Type type, Type? nullableUnderlyingType)
        {
            type = nullableUnderlyingType ?? type;
            return type == typeof(float)
                   || type == typeof(double)
                   || type == typeof(decimal);
        }
    }

    public static bool IsNullable(this Type type)
    {
        return type.GetTypeInfoEx().IsNullable;
    }

    public static Type UnwrapNullable(this Type type)
    {
        return type.GetTypeInfoEx().NullableUnderlyingType ?? type;
    }

    public static object? DefaultValue(this Type type)
    {
        return type.GetTypeInfoEx().DefaultValue;
    }

    public static Type? NullableType(this Type type)
    {
        return type.GetTypeInfoEx().NullableUnderlyingType;
    }

    public static Type? EnumerableType(this Type type)
    {
        return type.GetTypeInfoEx().EnumerableUnderlyingType;
    }

    /// <summary>
    /// Get type name without any generics info
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string SimpleName(this Type type)
    {
        return type.GetTypeInfoEx().SimpleName;
    }

    /// <summary>
    /// Get name of type with generic parameters without namespace.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string ShortName(this Type type)
    {
        return type.GetTypeInfoEx().ShortName;
    }

    /// <summary>
    /// Get name of type with generic parameters with namespace.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string LongName(this Type type)
    {
        return type.GetTypeInfoEx().LongName;
    }

    public static bool IsInteger(this Type type)
    {
        return type.GetTypeInfoEx().IsInteger;
    }

    public static bool IsNumeric(this Type type)
    {
        return type.GetTypeInfoEx().IsNumeric;
    }

    public static bool IsFloat(this Type type)
    {
        return type.GetTypeInfoEx().IsFloat;
    }

    public static bool IsEnumerable(this Type type)
    {
        return type.GetTypeInfoEx().IsEnumerable;
    }
}

public record TypeInfoEx(
    Type Type,
    Type? NullableUnderlyingType,
    Type? EnumerableUnderlyingType,
    object? DefaultValue,
    string SimpleName,
    string ShortName,
    string LongName,
    bool IsInteger,
    bool IsFloat)
{
    public bool IsNullable { get; } = NullableUnderlyingType != null;
    public bool IsEnumerable { get; } = EnumerableUnderlyingType != null;
    public bool IsNumeric { get; } = IsInteger || IsFloat;
}