namespace FclEx.Extensions;

partial class TypeExtensions
{
    private static readonly ConditionalWeakTable<Type, TypeInfoEx> _typeInfoCache = new();

    /// <summary>
    /// Gets cached reflection metadata and derived type facts used by the other type extension methods.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>A cached <see cref="TypeInfoEx"/> instance for <paramref name="type"/>.</returns>
    public static TypeInfoEx GetTypeInfoEx(this Type type)
    {
        FclEx.Check.NotNull(type);
        return _typeInfoCache.GetValue(type, m => new TypeInfoEx(m));
    }

    /// <summary>
    /// Determines whether the type is a constructed <see cref="Nullable{T}"/> value type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// <see langword="true"/> for <c>Nullable&lt;T&gt;</c>; otherwise, <see langword="false"/>.
    /// Nullable reference type annotations are compile-time metadata and are not detected by this method.
    /// </returns>
    public static bool IsNullable(this Type type)
    {
        return type.GetTypeInfoEx().IsNullable;
    }

    /// <summary>
    /// Returns the underlying value type for <see cref="Nullable{T}"/>; otherwise, returns the original type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>The nullable underlying type, or <paramref name="type"/> when it is not <see cref="Nullable{T}"/>.</returns>
    public static Type UnwrapNullable(this Type type)
    {
        return type.NullableUnderlyingType() ?? type;
    }

    /// <summary>
    /// Gets the default CLR value for the specified type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// The default value for value types, or <see langword="null"/> for reference types, nullable value types,
    /// open generic types, <see langword="void"/>, typed references, and by-ref-like types.
    /// </returns>
    /// <remarks>
    /// Value type defaults are created without invoking a user-defined parameterless constructor.
    /// </remarks>
    public static object? DefaultValue(this Type type)
    {
        return type.GetTypeInfoEx().DefaultValue;
    }

    /// <summary>
    /// Gets the underlying value type for a constructed <see cref="Nullable{T}"/> type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>The nullable underlying type, or <see langword="null"/> when <paramref name="type"/> is not <see cref="Nullable{T}"/>.</returns>
    public static Type? NullableUnderlyingType(this Type type)
    {
        return type.GetTypeInfoEx().NullableUnderlyingType;
    }

    /// <summary>
    /// Gets the element types exposed by arrays and enumerable types.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IReadOnlyList<Type> EnumerableElementTypes(this Type type)
    {
        return type.GetTypeInfoEx().EnumerableElementTypes;
    }

    /// <summary>
    /// Gets the element type exposed by arrays and enumerable types.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// The array element type, the generic argument of <see cref="IEnumerable{T}"/>, <see cref="object"/> for
    /// non-generic <see cref="IEnumerable"/>, or <see langword="null"/> when the type is not enumerable.
    /// </returns>
    public static Type? EnumerableElementType(this Type type)
    {
        var types = type.GetTypeInfoEx().EnumerableElementTypes;
        return types.Count switch
        {
            0 => null,
            1 => types[0],
            _ => throw new AmbiguousMatchException($"Type '{type}' implements multiple enumerable element types: {string.Join(", ", types)}"),
        };
    }

    /// <summary>
    /// Gets the type name without namespace or generic argument information.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// The simple metadata name. For generic types, the arity suffix is removed; for example,
    /// <c>Dictionary&lt;string, int&gt;</c> returns <c>Dictionary</c>.
    /// </returns>
    public static string SimpleName(this Type type)
    {
        return type.GetTypeInfoEx().SimpleName;
    }

    /// <summary>
    /// Gets the type name without namespace, including formatted generic arguments.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// The short display name. For example, <c>Dictionary&lt;string, int&gt;</c> returns
    /// <c>Dictionary&lt;String, Int32&gt;</c>.
    /// </returns>
    public static string ShortName(this Type type)
    {
        return type.GetTypeInfoEx().ShortName;
    }

    /// <summary>
    /// Gets the type name with namespace and formatted generic arguments.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// The fully qualified display name. Nested types are separated with a period, and generic type or method
    /// parameters are qualified by their declaring member.
    /// </returns>
    public static string LongName(this Type type)
    {
        return type.GetTypeInfoEx().LongName;
    }

    /// <summary>
    /// Determines whether the type is an integer type, or a nullable integer type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><see langword="true"/> for signed, unsigned, and native-sized integral types; otherwise, <see langword="false"/>.</returns>
    public static bool IsInteger(this Type type)
    {
        return type.GetTypeInfoEx().IsInteger;
    }

    /// <summary>
    /// Determines whether the type is an integer or floating-point numeric type, or a nullable numeric type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><see langword="true"/> for supported numeric types; otherwise, <see langword="false"/>.</returns>
    public static bool IsNumeric(this Type type)
    {
        return type.GetTypeInfoEx().IsNumeric;
    }

    /// <summary>
    /// Determines whether the type is a floating-point numeric type, or a nullable floating-point numeric type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><see langword="true"/> for <see cref="float"/>, <see cref="double"/>, <see cref="decimal"/>, and their nullable forms; otherwise, <see langword="false"/>.</returns>
    public static bool IsFloatingPoint(this Type type)
    {
        return type.GetTypeInfoEx().IsFloatingPoint;
    }

    /// <summary>
    /// Determines whether the type is an array or implements <see cref="IEnumerable"/>.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><see langword="true"/> when <see cref="EnumerableElementType"/> returns a non-null type; otherwise, <see langword="false"/>.</returns>
    public static bool IsEnumerable(this Type type)
    {
        return type.GetTypeInfoEx().IsEnumerable;
    }
}

/// <summary>
/// Contains cached reflection metadata and derived type facts for a <see cref="Type"/>.
/// </summary>
/// <param name="Type">The inspected type.</param>
/// <param name="NullableUnderlyingType">The underlying value type for <see cref="Nullable{T}"/>, or <see langword="null"/>.</param>
/// <param name="EnumerableElementTypes">The element types exposed by arrays and enumerable types, or an empty array.</param>
/// <param name="DefaultValue">The default CLR value for value types, or <see langword="null"/> for types without a boxed default value.</param>
/// <param name="SimpleName">The type name without namespace or generic argument information.</param>
/// <param name="ShortName">The type name without namespace, including formatted generic arguments.</param>
/// <param name="LongName">The type name with namespace and formatted generic arguments.</param>
/// <param name="IsInteger">Whether the type is an integer type or nullable integer type.</param>
/// <param name="IsFloatingPoint">Whether the type is a floating-point type, decimal type, or nullable form of either.</param>
public class TypeInfoEx : IEquatable<TypeInfoEx>
{
    public TypeInfoEx(Type type)
    {
        Type = type;
        NullableUnderlyingType = Nullable.GetUnderlyingType(type);
        DefaultValue = GetDefaultValueCore(type, NullableUnderlyingType);
        EnumerableElementTypes = GetEnumerableElementTypesCore(type).NotNull().ToReadOnlyList();
        SimpleName = GetSimpleNameCore(type);
        ShortName = GetShortNameCore(type, SimpleName);
        LongName = GetLongNameCore(type, ShortName);
        IsInteger = IsIntegerCore(type, NullableUnderlyingType);
        IsFloatingPoint = IsFloatingPointCore(type, NullableUnderlyingType);
        IsNullable = NullableUnderlyingType != null;
        IsEnumerable = EnumerableElementTypes.Count > 0;
        IsNumeric = IsInteger
                    || IsFloatingPoint
                    || type == typeof(decimal)
                    || type == typeof(BigInteger);
    }

    /// <summary>The inspected type.</summary>
    public readonly Type Type;

    /// <summary>The underlying value type for <see cref="Nullable{T}"/>, or <see langword="null"/>.</summary>
    public readonly Type? NullableUnderlyingType;

    /// <summary>The element types exposed by arrays and enumerable types, or an empty array.</summary>
    public readonly IReadOnlyList<Type> EnumerableElementTypes;

    /// <summary>The default CLR value for value types, or <see langword="null"/> for types without a boxed default value.</summary>
    public readonly object? DefaultValue;

    /// <summary>The type name without namespace or generic argument information.</summary>
    public readonly string SimpleName;

    /// <summary>The type name without namespace, including formatted generic arguments.</summary>
    public readonly string ShortName;

    /// <summary>The type name with namespace and formatted generic arguments.</summary>
    public readonly string LongName;

    /// <summary>Whether the type is an integer type or nullable integer type.</summary>
    public readonly bool IsInteger;

    /// <summary>Whether the type is a floating-point type, decimal type, or nullable form of either.</summary>
    public readonly bool IsFloatingPoint;

    /// <summary>Gets a value indicating whether <see cref="Type"/> is a constructed <see cref="Nullable{T}"/> value type.</summary>
    public readonly bool IsNullable;

    /// <summary>Gets a value indicating whether <see cref="Type"/> is an array or implements <see cref="IEnumerable"/>.</summary>
    public readonly bool IsEnumerable;

    /// <summary>Gets a value indicating whether <see cref="Type"/> is an integer or floating-point numeric type.</summary>
    public readonly bool IsNumeric;

#if !NET5_0_OR_GREATER
    private static readonly Lazy<PropertyInfo?> _isByRefLike = new(() => typeof(Type).GetProperty("IsByRefLike", BindingAttributes.Declared));

    private static bool IsByRefLike(Type type)
    {
        return _isByRefLike.Value is { } field && field.GetValue<bool>(type);
    }
#endif

    private static object? GetDefaultValueCore(Type type, Type? nullableUnderlyingType)
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
            || type.FullName is "System.ArgIterator" or "System.RuntimeArgumentHandle" or "System.TypedReference"
            // Byref-like structures are declared using ref struct keyword in C#. 
#if !NET5_0_OR_GREATER
            || IsByRefLike(type)
#else
                || type.IsByRefLike
#endif
            || type == typeof(void))
            return null;

        try
        {
            return ObjectHelper.GetUninitializedObject(type);
        }
        catch (Exception ex)
        {
            Trace.TraceWarning($"Failed to create instance of type {type.FullName}: {ex}");
            return null;
        }
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    private static IEnumerable<Type?> GetEnumerableElementTypesCore(Type type)
    {
        // type is Array
        if (type.IsArray)
            return [type.GetElementType()!];

        // type is IEnumerable<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var t = type.GenericTypeArguments.FirstOrDefault()
                    ?? type.GetTypeInfo().GenericTypeParameters.FirstOrDefault();
            return [t];
        }

        // type implements IEnumerable<T>
        if (type.GetImplementedInterfaces(typeof(IEnumerable<>)) is { Length: > 0 } iEnumerableTypes)
            return iEnumerableTypes.Select(t => t.GenericTypeArguments[0]);

        // type implements IEnumerable
        if (type.IsAssignableTo(typeof(IEnumerable)))
            return [typeof(object)];

        return [];
    }

    private static string GetSimpleNameCore(Type type)
    {
        var name = type.Name;

        if (type.IsGenericType == false)
            return name;

        var index = name.IndexOf('`');
        return index == -1 ? name : name[..index];
    }

    private static string GetShortNameCore(Type type, string simpleName)
    {
        if (!type.IsGenericType) return type.Name;
        var paraName = string.Join(", ", type.GenericTypeArguments!.Select(m => m.ShortName()));
        return simpleName + "<" + paraName + ">";
    }

    private static string GetLongNameCore(Type type, string shortName)
    {
        return GetTypePrefixCore(type) + shortName;
    }

    private static string GetTypePrefixCore(Type type)
    {
        if (type.IsNested)
        {
            var declaringType = type.DeclaringType!;
            return GetTypePrefixCore(declaringType) + declaringType.ShortName() + ".";
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

    private static bool IsIntegerCore(Type type, Type? nullableUnderlyingType)
    {
        type = nullableUnderlyingType ?? type;
        return type == typeof(long)
               || type == typeof(ulong)
               || type == typeof(int)
               || type == typeof(uint)
               || type == typeof(short)
               || type == typeof(ushort)
               || type == typeof(byte)
               || type == typeof(sbyte)
               || type == typeof(nint)
               || type == typeof(nuint)
#if NET7_0_OR_GREATER
               || type == typeof(Int128)
               || type == typeof(UInt128)
#endif
               ;
    }

    private static bool IsFloatingPointCore(Type type, Type? nullableUnderlyingType)
    {
        type = nullableUnderlyingType ?? type;
        return type == typeof(float)
               || type == typeof(double)
#if NET5_0_OR_GREATER
               || type == typeof(Half)
#endif
               ;
    }

    public bool Equals(TypeInfoEx? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Type == other.Type;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TypeInfoEx)obj);
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }
}
