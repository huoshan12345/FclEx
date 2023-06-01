using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FclEx.Attributes;

namespace FclEx.Extensions;

public static class EnumExtensions
{
    public static TInteger ToInteger<TEnum, TInteger>(this TEnum enumValue)
        where TEnum : struct, Enum
        where TInteger : unmanaged
    {
        if (enumValue.TryToInteger(out TInteger value))
        {
            return value;
        }
        throw new InvalidCastException($"Cannot cast {typeof(TEnum).Name} to {typeof(TInteger).Name}");
    }

    public static bool TryToInteger<TEnum, TInteger>(this TEnum enumValue, out TInteger integer)
        where TEnum : struct, Enum
        where TInteger : unmanaged
    {
        if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<TInteger>())
        {
            integer = Unsafe.As<TEnum, TInteger>(ref enumValue);
            return true;
        }
        else
        {
            integer = default;
            return false;
        }
    }

    public static string ToIntStr(this Enum enumValue)
    {
        return enumValue.ToInt().ToString();
    }

    public static int ToInt<TEnum>(this TEnum enumValue) where TEnum : Enum
    {
        // The approved types for an enum are: 
        // byte, sbyte,
        // short, ushort,
        // int, uint,
        // long, ulong. 
        var size = Unsafe.SizeOf<TEnum>();
        return size switch
        {
            1 => Unsafe.As<TEnum, byte>(ref enumValue),
            2 => Unsafe.As<TEnum, short>(ref enumValue),
            4 => Unsafe.As<TEnum, int>(ref enumValue),
            _ => throw new InvalidCastException($"Cannot cast {typeof(TEnum).Name} to int")
        };
    }

    public static long ToLong<TEnum>(this TEnum enumValue) where TEnum : Enum
    {
        var size = Unsafe.SizeOf<TEnum>();
        return size switch
        {
            1 => Unsafe.As<TEnum, byte>(ref enumValue),
            2 => Unsafe.As<TEnum, short>(ref enumValue),
            4 => Unsafe.As<TEnum, int>(ref enumValue),
            8 => Unsafe.As<TEnum, long>(ref enumValue),
            _ => throw new InvalidCastException($"Cannot cast {typeof(TEnum).Name} to long")
        };
    }

    public static T ToEnum<T>(this string? value, T defaultValue) where T : struct, Enum
    {
        return value.ToEnum(s => defaultValue);
    }

    public static T ToEnum<T>(this string? value, Func<string?, T> defaultValueFunc) where T : struct, Enum
    {
        return Enum.TryParse<T>(value, true, out var result) ? result : defaultValueFunc(value);
    }

    public static T ToEnum<T>(this string? value) where T : struct, Enum
    {
        return value.ToEnum<T>(s => throw new FormatException($"Cannot parse to type of {typeof(T).ShortName()} from this value: " + s));
    }

    public static bool IsValid<T>(this T value) where T : struct, Enum
    {
        var validValues = Enum.GetValues<T>();
        return validValues.Contains(value);
    }

    public static bool IsEachValid<T>(this IEnumerable<T> values) where T : struct, Enum
    {
        var validValues = Enum.GetValues<T>();
        return values.All(m => validValues.Contains(m));
    }

    public static TEnum IfNotValid<TEnum>(this TEnum e, TEnum defaultValue = default) where TEnum : struct, Enum
    {
        return e.IsValid() ? e : defaultValue;
    }

    private static readonly ConcurrentDictionary<Enum, string> EnumValueDic = new();

    public static TAttr? GetAttribute<TAttr>(this Enum e) where TAttr : Attribute
    {
        var type = e.GetType();
        var field = type.GetField(e.ToString())!;
        var attr = field.GetCustomAttribute<TAttr>(false);
        return attr;
    }

    public static string GetValue(this Enum e)
    {
        return EnumValueDic.GetOrAdd(e, m => m.GetAttribute<EnumValueAttribute>()?.Value ?? e.ToString());
    }
}