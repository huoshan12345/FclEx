namespace FclEx.Helpers;

public record EnumInfo(string Name, string Lower, string Upper, long Value, string? EnumMemberValue);

public static class EnumHelper
{
    private static readonly ConcurrentDictionary<Type, EnumInfo[]> _infos = new();

    public static EnumInfo[] GetInfos<T>() where T : struct, Enum
    {
        return _infos.GetOrAdd(typeof(T), m => Enum.GetValues<T>().Select(x => x.Info()).ToArray());
    }

    public static bool TryParse<TEnum>([NotNullWhen(true)] string? value, bool ignoreCase, bool fromNumeric, out TEnum result) where TEnum : struct, Enum
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

        var cmp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
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