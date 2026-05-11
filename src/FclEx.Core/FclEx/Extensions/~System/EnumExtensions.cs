namespace FclEx.Extensions;

public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<Enum, EnumInfo> _infos = new();

    public static EnumInfo Info(this Enum enumValue)
    {
        return _infos.GetOrAdd(enumValue, k =>
        {
            var name = k.ToString();
            return new(
                name,
                name.ToLower(),
                name.ToUpper(),
                k.CastTo<long>(),
                k.GetAttribute<EnumMemberAttribute>()?.Value);
        });
    }

    public static string MemberValue(this Enum e)
    {
        var info = e.Info();
        return info.EnumMemberValue ?? info.Name;
    }

    public static string ToLower(this Enum enumValue)
    {
        return enumValue.Info().Lower;
    }

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

    public static bool IsValid<T>(this T value) where T : struct, Enum
    {
        var validValues = (T[])Enum.GetValues(typeof(T));
        return validValues.Contains(value);
    }

    public static T? GetAttribute<T>(this Enum enumValue) where T : Attribute
    {
        var type = enumValue.GetType();
        var field = type.GetField(enumValue.ToString())!;
        var attr = field.GetCustomAttribute<T>(false);
        return attr;
    }

    /// <summary>
    /// A generic and more efficient implement of <see cref="Enum.HasFlag" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="e"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    public static bool IsSet<T>(this T e, T flag) where T : struct, Enum
    {
        var enumInt = e.ToLong();
        var flagInt = flag.ToLong();
        return (enumInt & flagInt) == flagInt;
    }

    extension(Enum)
    {
#if NETSTANDARD2_0
        public static T[] GetValues<T>() where T : struct, Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }
#endif
    }
}