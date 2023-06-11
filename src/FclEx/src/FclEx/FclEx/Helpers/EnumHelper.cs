namespace FclEx.Helpers;

public record EnumInfo(string Name, string Lower, string Upper, long Value);

public static class EnumHelper
{
    private static readonly ConcurrentDictionary<Type, EnumInfo[]> _infos = new();

    public static EnumInfo[] GetInfos<TEnum>() where TEnum : struct, Enum
    {
        return _infos.GetOrAdd(typeof(TEnum), m => Enum.GetValues<TEnum>().Select(m => m.Info()).ToArray());
    }

    public static bool TryParse<TEnum>([NotNullWhen(true)] string? value, bool ignoreCase, bool fromNumeric, out TEnum result) 
        where TEnum : struct, Enum
    {
        if (fromNumeric && long.TryParse(value, out var number))
        {
            var e = number.CastTo<TEnum>();
            if (Enum.IsDefined(e))
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
}