using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FclEx.Extensions;

partial class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, TypeInfoExt> TypeInfoDic = new();

    public static TypeInfoExt GetTypeInfoExt(this Type type)
    {
        Check.NotNull(type);
        return TypeInfoDic.GetOrAdd(type, GetTypeInfoExtInternal);

        static TypeInfoExt GetTypeInfoExtInternal(Type type)
        {
            var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            var defaultValue = GetDefaultValueInternal(type, nullableUnderlyingType);
            var enumerableUnderlyingType = GetEnumerableUnderlyingTypeInternal(type);
            var simpleName = SimpleNameInternal(type);
            var shortName = ShortNameInternal(type, simpleName);
            var longName = LongNameInternal(type, shortName);
            var isInteger = IsIntegerInternal(type, nullableUnderlyingType);
            var isFloat = IsFloatInternal(type, nullableUnderlyingType);

            return new TypeInfoExt(type, nullableUnderlyingType, enumerableUnderlyingType, defaultValue, simpleName, shortName, longName, isInteger, isFloat);
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

            if (!type.IsValueType
                || nullableUnderlyingType != null
                || type.ContainsGenericParameters
                || type.Name == "ArgIterator"
                || type == typeof(void)
                || type.IsByRefLike)
                return null;

            return Activator.CreateInstance(type);
        }

        static Type? GetEnumerableUnderlyingTypeInternal(Type type)
        {
            // Type is Array
            // short-circuit if you expect lots of arrays 
            if (type.IsArray)
                return type.GetElementType();

            // type is IEnumerable<T>;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return type.GenericTypeArguments.FirstOrDefault() ?? type.GetTypeInfo().GenericTypeParameters.FirstOrDefault();
            }

            // type implements/extends IEnumerable<T>;
            var enumType = type.GetGenericInterface(typeof(IEnumerable<>));
            if (enumType != null)
                return enumType.GenericTypeArguments![0];

            return null;
        }

        static string SimpleNameInternal(Type type)
        {
            if (!type.IsGenericType) return type.Name.ToStringOrEmpty();
            var name = type.Name.ToStringOrEmpty();
            var index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
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
        return type.GetTypeInfoExt().IsNullable;
    }

    public static Type UnwarpNullable(this Type type)
    {
        return type.GetTypeInfoExt().NullableUnderlyingType ?? type;
    }

    public static object? DefaultValue(this Type type)
    {
        return type.GetTypeInfoExt().DefaultValue;
    }

    public static Type? NullableType(this Type type)
    {
        return type.GetTypeInfoExt().NullableUnderlyingType;
    }

    public static Type? EnumerableType(this Type type)
    {
        return type.GetTypeInfoExt().EnumerableUnderlyingType;
    }

    /// <summary>
    /// Get type name without any generics info
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string SimpleName(this Type type)
    {
        return type.GetTypeInfoExt().SimpleName;
    }

    /// <summary>
    /// Get name of type with generic parameters without namespace.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string ShortName(this Type type)
    {
        return type.GetTypeInfoExt().ShortName;
    }

    /// <summary>
    /// Get name of type with generic parameters with namespace.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string LongName(this Type type)
    {
        return type.GetTypeInfoExt().LongName;
    }

    public static bool IsInteger(this Type type)
    {
        return type.GetTypeInfoExt().IsInteger;
    }

    public static bool IsNumeric(this Type type)
    {
        return type.GetTypeInfoExt().IsNumeric;
    }

    public static bool IsFloat(this Type type)
    {
        return type.GetTypeInfoExt().IsFloat;
    }

    public static bool IsEnumerable(this Type type)
    {
        return type.GetTypeInfoExt().IsEnumerable;
    }
}

public class TypeInfoExt
{
    public readonly Type Type;
    public readonly Type? NullableUnderlyingType;
    public readonly Type? EnumerableUnderlyingType;
    public readonly object? DefaultValue;
    public readonly bool IsNullable;
    public readonly bool IsEnumerable;
    public readonly string SimpleName;
    public readonly string ShortName;
    public readonly string LongName;
    public readonly bool IsInteger;
    public readonly bool IsNumeric;
    public readonly bool IsFloat;

    public TypeInfoExt(Type type,
        Type? nullableUnderlyingType,
        Type? enumerableUnderlyingType,
        object? defaultValue,
        string simpleName,
        string shortName,
        string longName,
        bool isInteger,
        bool isFloat)
    {
        Type = type;
        NullableUnderlyingType = nullableUnderlyingType;
        DefaultValue = defaultValue;
        SimpleName = simpleName;
        ShortName = shortName;
        LongName = longName;
        IsInteger = isInteger;
        IsFloat = isFloat;
        EnumerableUnderlyingType = enumerableUnderlyingType;

        IsNullable = nullableUnderlyingType != null;
        IsEnumerable = enumerableUnderlyingType != null || typeof(IEnumerable).IsAssignableFrom(type);
        IsNumeric = IsInteger || IsFloat;
    }
}