namespace FclEx.Extensions;

public static partial class ObjectExtensions
{
    [MethodImpl(AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(obj))]
    public static T? CastTo<T>(this object? obj)
    {
        return obj is null ? default : (T)(dynamic)obj;
    }

    /// <summary>
    /// Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.
    /// </summary>
    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
    {
        var cmpMin = value.CompareTo(min);
        if (cmpMin <= 0) // value <= min
            return min;

        var cmpMax = value.CompareTo(max);
        if (cmpMax >= 0) // value >= max
            return max;

        return value;
    }

    [MethodImpl(AggressiveInlining)]
    public static T? ToNullable<T>(this T value) where T : struct
    {
        return value;
    }
}