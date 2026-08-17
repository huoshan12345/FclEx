namespace FclEx.Extensions;

public static partial class ObjectExtensions
{
    [MethodImpl(AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(obj))]
    public static T? CastTo<T>(this object? obj)
    {
        return obj switch
        {
            null => default,
            T t => t,
            _ => ChangeType(obj),
        };

        static T ChangeType(object obj)
        {
            var type = typeof(T);
            var targetType = Nullable.GetUnderlyingType(type) ?? type;
            return targetType.IsEnum
                ? (T)Enum.ToObject(targetType, obj)
                : (T)Convert.ChangeType(obj, targetType);
        }
    }

    /// <summary>
    /// Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.
    /// </summary>
    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
    {
        Check.NotGreaterThan(min, max);

        var cmpMin = value.CompareTo(min);
        if (cmpMin <= 0) // value <= min
            return min;

        var cmpMax = value.CompareTo(max);
        return cmpMax >= 0 ? // value >= max
            max : value;
    }

    [MethodImpl(AggressiveInlining)]
    public static T? ToNullable<T>(this T value) where T : struct
    {
        return value;
    }
}