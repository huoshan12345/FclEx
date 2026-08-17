namespace FclEx.Extensions;

public static partial class ObjectExtensions
{
    /// <summary>
    /// Returns <paramref name="obj"/> when it is already <typeparamref name="T"/>, or converts it to that type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="obj">The source value.</param>
    /// <returns>The converted value, or the default value of <typeparamref name="T"/> when <paramref name="obj"/> is <see langword="null"/>.</returns>
    /// <remarks>
    /// Non-enum conversions use <see cref="Convert.ChangeType(object, Type)"/>; enum conversions use
    /// <see cref="Enum.ToObject(Type, object)"/>. User-defined conversion operators are not invoked.
    /// </remarks>
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

    /// <summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
    /// <exception cref="ArgumentException"><paramref name="min"/> is greater than <paramref name="max"/>.</exception>
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
