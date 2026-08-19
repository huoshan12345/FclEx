namespace FclEx.Extensions;

/// <summary>
/// Contains cached reflection metadata and derived type facts for a <see cref="Type"/>.
/// </summary>
public sealed class TypeInfoEx : IEquatable<TypeInfoEx>
{
    /// <summary>
    /// Initializes metadata and derived type facts for the specified type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    public TypeInfoEx(Type type)
    {
        Type = type;
        NullableUnderlyingType = Nullable.GetUnderlyingType(type);
        NonNullableType = NullableUnderlyingType ?? type;
        IsNullable = NullableUnderlyingType != null;
        DefaultValue = type.GetDefaultValue();
        SimpleName = type.GetSimpleName();
        ShortName = type.GetShortName(SimpleName);
        LongName = type.GetLongName(ShortName);
        EnumerableElementTypes = NonNullableType.GetEnumerableElementTypes().NotNull().ToReadOnlyList();
        IsEnumerable = EnumerableElementTypes.Count > 0;
        IsInteger = Extensions.IsInteger(NonNullableType);
        IsFloatingPoint = Extensions.IsFloatingPoint(NonNullableType);
        IsNumeric = IsInteger
                    || IsFloatingPoint
                    || NonNullableType == typeof(decimal)
                    || NonNullableType == typeof(BigInteger);
    }

    /// <summary>The inspected type.</summary>
    public readonly Type Type;

    /// <summary>The underlying value type for <see cref="Nullable{T}"/>, or <see langword="null"/>.</summary>
    public readonly Type? NullableUnderlyingType;

    /// <summary> Gets the underlying value type for <see cref="Nullable{T}"/>, or the original type if it is not nullable.</summary>
    public readonly Type NonNullableType;

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

    /// <summary>Whether the type is a binary floating-point type or a nullable form of one.</summary>
    public readonly bool IsFloatingPoint;

    /// <summary>Gets a value indicating whether <see cref="Type"/> is a constructed <see cref="Nullable{T}"/> value type.</summary>
    public readonly bool IsNullable;

    /// <summary>Gets a value indicating whether <see cref="Type"/> is an array or implements <see cref="IEnumerable"/>.</summary>
    public readonly bool IsEnumerable;

    /// <summary>Gets a value indicating whether <see cref="Type"/> is one of the supported numeric types.</summary>
    public readonly bool IsNumeric;

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

file static class Extensions
{
#if !NET5_0_OR_GREATER
    private static readonly Lazy<PropertyInfo?> _isByRefLike = new(() => typeof(Type).GetProperty("IsByRefLike", BindingAttributes.Declared));

    private static bool IsByRefLike(Type type)
    {
        return _isByRefLike.Value is { } field && field.GetValue<bool>(type);
    }
#endif

    public static object? GetDefaultValue(this Type type)
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
            || Nullable.GetUnderlyingType(type) is not null
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
            return RuntimeHelpers.GetUninitializedObject(type);
        }
        catch (Exception ex)
        {
            Trace.TraceWarning($"Failed to create instance of type {type.FullName}: {ex}");
            return null;
        }
    }
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public static IEnumerable<Type?> GetEnumerableElementTypes(this Type type)
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

    public static string GetSimpleName(this Type type)
    {
        if (type.IsArray)
            return type.GetElementType()!.SimpleName() + type.GetArraySuffix();

        var name = type.Name;

        if (type.IsGenericType == false)
            return name;

        var index = name.IndexOf('`');
        return index == -1 ? name : name[..index];
    }

    public static string GetShortName(this Type type, string? simpleName)
    {
        if (type.IsArray)
            return type.GetElementType()!.GetShortName(null) + type.GetArraySuffix();

        simpleName ??= type.GetSimpleName();
        var genericArguments = type.GetOwnGenericArguments();
        return genericArguments.Length == 0
            ? simpleName
            : simpleName + "<" + string.Join(", ", genericArguments.Select(GetGenericArgumentShortName)) + ">";
    }

    public static string GetLongName(this Type type, string? shortName)
    {
        if (type.IsArray)
            return type.GetElementType()!.GetLongName(null) + type.GetArraySuffix();

        if (type is { IsNested: true, IsGenericParameter: false })
            return type.GetNestedLongName();

        shortName ??= type.GetShortName(null);
        return type.GetTypePrefix() + shortName;
    }

    private static string GetNestedLongName(this Type type)
    {
        var declaringTypes = new Stack<Type>();
        for (var current = type; current is not null; current = current.DeclaringType)
            declaringTypes.Push(current);

        var genericArguments = type.GetGenericArguments();
        var genericArgumentIndex = 0;
        var names = new List<string>(declaringTypes.Count);
        Type? outermostType = null;

        while (declaringTypes.Count > 0)
        {
            var current = declaringTypes.Pop();
            outermostType ??= current;
            var arity = current.GetDeclaredGenericArity();
            var arguments = genericArguments.Skip(genericArgumentIndex).Take(arity).ToArray();
            genericArgumentIndex += arity;

            var name = current.GetSimpleName();
            if (arguments.Length > 0)
                name += "<" + string.Join(", ", arguments.Select(GetGenericArgumentShortName)) + ">";

            names.Add(name);
        }

        var prefix = outermostType!.GetNamespacePrefix();
        return prefix + string.Join(".", names);
    }

    private static Type[] GetOwnGenericArguments(this Type type)
    {
        var arity = type.GetDeclaredGenericArity();
        if (arity == 0)
            return [];

        var arguments = type.GetGenericArguments();
        return arguments.Skip(arguments.Length - arity).ToArray();
    }

    private static string GetGenericArgumentShortName(this Type type)
    {
        // Formatting a generic parameter through the TypeInfoEx cache would re-enter formatting
        // of its declaring open generic type before either cache entry has finished construction.
        return type.IsGenericParameter
            ? type.Name
            : type.GetShortName(null);
    }

    private static int GetDeclaredGenericArity(this Type type)
    {
        var separatorIndex = type.Name.LastIndexOf('`');
        return separatorIndex < 0
            ? 0
            : int.Parse(type.Name[(separatorIndex + 1)..], CultureInfo.InvariantCulture);
    }

    private static string GetArraySuffix(this Type type)
    {
        if (type.GetArrayRank() == 1)
            return type == type.GetElementType()!.MakeArrayType() ? "[]" : "[*]";

        return "[" + new string(',', type.GetArrayRank() - 1) + "]";
    }

    private static string GetNamespacePrefix(this Type type)
    {
        return type.Namespace is { } ns
            ? ns + "."
            : "global::";
    }

    private static string GetTypePrefix(this Type type)
    {
        if (type.IsGenericParameter)
        {
            var declaringType = type.DeclaringType!;
            var longName = declaringType.GetLongName(null);
            return type.DeclaringMethod is { } declaringMethod
                ? longName + "." + declaringMethod.Name + "."
                : longName + ".";
        }

        // ReSharper disable once InvertIf
        if (type.IsNested)
        {
            var declaringType = type.DeclaringType!;
            return declaringType.GetTypePrefix() + declaringType.GetShortName(null) + ".";
        }

        return type.GetNamespacePrefix();
    }

    public static bool IsInteger(this Type type)
    {
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

    public static bool IsFloatingPoint(this Type type)
    {
        return type == typeof(float)
               || type == typeof(double)
#if NET5_0_OR_GREATER
               || type == typeof(Half)
#endif
            ;
    }
}
