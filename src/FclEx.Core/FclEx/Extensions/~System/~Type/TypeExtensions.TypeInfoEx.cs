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
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// All distinct element types exposed through <see cref="IEnumerable{T}"/>, the array element type,
    /// <see cref="object"/> for a non-generic enumerable, or an empty list when the type is not enumerable.
    /// </returns>
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
    /// Determines whether the type is a supported numeric type, or a nullable form of one.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// <see langword="true"/> for integral and binary floating-point types, <see cref="decimal"/>,
    /// <see cref="BigInteger"/>, and their nullable forms; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsNumeric(this Type type)
    {
        return type.GetTypeInfoEx().IsNumeric;
    }

    /// <summary>
    /// Determines whether the type is a floating-point numeric type, or a nullable floating-point numeric type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// <see langword="true"/> for <see cref="float"/>, <see cref="double"/>, <c>Half</c> where available,
    /// and their nullable forms; otherwise, <see langword="false"/>.
    /// </returns>
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