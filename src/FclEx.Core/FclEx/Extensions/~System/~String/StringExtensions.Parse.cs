namespace FclEx.Extensions;

partial class StringExtensions
{
    public static int ToInt(this string? str, int defaultValue = default,
        NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null)
    {
        return int.TryParse(str, style, provider, out var r) ? r : defaultValue;
    }

    public static bool ToBool(this string? str, bool defaultValue = default)
    {
        return bool.TryParse(str, out var r) ? r : defaultValue;
    }

    public static double ToDouble(this string? str, double defaultValue = default, NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands, IFormatProvider? provider = null)
    {
        return double.TryParse(str, style, provider, out var r) ? r : defaultValue;
    }

    public static DateTime ToDateTime(this string? str, DateTime defaultValue = default, IFormatProvider? provider = null, DateTimeStyles styles = DateTimeStyles.None)
    {
        return DateTime.TryParse(str, provider, styles, out var r) ? r : defaultValue;
    }
}