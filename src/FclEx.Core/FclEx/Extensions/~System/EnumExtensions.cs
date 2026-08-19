namespace FclEx.Extensions;

public record EnumInfo(string Name, string Lower, string Upper, long Value, string? EnumMemberValue);

public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<Enum, EnumInfo> _valueCache = new();
    private static readonly ConditionalWeakTable<Type, EnumInfo[]> _typeCache = new();

    public static EnumInfo Info(this Enum enumValue)
    {
        return Enum.IsDefined(enumValue.GetType(), enumValue)
            ? _valueCache.GetOrAdd(enumValue, CreateEnumInfo)
            : CreateEnumInfo(enumValue);

        static EnumInfo CreateEnumInfo(Enum enumValue)
        {
            var name = enumValue.ToString();
            return new EnumInfo(
                name,
                name.ToLowerInvariant(),
                name.ToUpperInvariant(),
                enumValue.CastTo<long>(),
                enumValue.GetAttribute<EnumMemberAttribute>()?.Value);
        }
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
        if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<TInteger>() && typeof(TInteger).IsInteger())
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
        var validValues = Enum.GetValues<T>();
        return validValues.Contains(value);
    }

    public static T? GetAttribute<T>(this Enum enumValue) where T : Attribute
    {
        var type = enumValue.GetType();
        var field = type.GetField(enumValue.ToString());
        var attr = field?.GetCustomAttribute<T>(false);
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
#if !NET5_0_OR_GREATER
        public static T[] GetValues<T>() where T : struct, Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static bool IsDefined<T>(T value) where T : struct, Enum
        {
            return Enum.IsDefined(typeof(T), value);
        }
#endif
        public static EnumInfo[] GetInfos<T>() where T : struct, Enum
        {
            return _typeCache.GetValue(typeof(T), m => Enum.GetValues(m).Cast<T>().Select(x => x.Info()).ToArray());
        }

        public static bool TryParse<TEnum>(
            [NotNullWhen(true)] string? value, 
            bool ignoreCase, 
            bool fromNumeric, 
            out TEnum result) where TEnum : struct, Enum
        {
            if (fromNumeric && long.TryParse(value, out var number))
            {
                var e = number.CastTo<TEnum>();
                if (Enum.IsDefined(typeof(TEnum), e))
                {
                    result = e;
                    return true;
                }
                else
                {
                    result = default;
                    return false;
                }
            }

            var cmp = ignoreCase
                ? StringComparison.OrdinalIgnoreCase 
                : StringComparison.Ordinal;

            var infos = GetInfos<TEnum>();
            var info = infos.SingleOrDefault(m => string.Equals(m.Name, value, cmp));
            if (info == null)
            {
                result = default;
                return false;
            }
            else
            {
                result = info.Value.CastTo<TEnum>();
                return true;
            }
        }

        public static T Parse<T>(string? value, T defaultValue) where T : struct, Enum
        {
            return Parse(value, s => defaultValue);
        }

        public static T Parse<T>(string? value, Func<string?, T> defaultValueFunc) where T : struct, Enum
        {
            return Enum.TryParse<T>(value, true, out var result) ? result : defaultValueFunc(value);
        }

        public static T Parse<T>(string? value) where T : struct, Enum
        {
            return Parse<T>(value, s => throw new FormatException($"Cannot parse to type of {typeof(T).ShortName()} from this value: " + s));
        }
    }
}