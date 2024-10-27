namespace FclEx.Extensions;

public static partial class ObjectExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(obj))]
    public static T? CastTo<T>(this object? obj)
    {
        return obj is null ? default : (T)(dynamic)obj;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStringOrEmpty<T>(this T? obj)
    {
        return obj?.ToString() ?? string.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetHashCodeSafely<T>(this T? obj)
    {
        return obj is null ? 0 : obj.GetHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(obj))]
    public static T? CloneByJson<T>(this T? obj, JsonSerializerOptions? options = null)
    {
        return obj is null ? obj : obj.ToJson(options).FromJson<T>(options);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDefault<T>(this T source)
    {
        return EqualityComparer<T>.Default.Equals(source, default!);
    }

    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
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

    private static long _nextId;
    private static readonly ConditionalWeakTable<object, object> _objectIds = new();
    public static long GetObjectId<T>(this T? obj) where T : class
    {
        return obj is null
            ? 0
            : (long)_objectIds.GetValue(obj, _ => Interlocked.Increment(ref _nextId));
    }

    public static DisposableValue<GCHandle> ToGCHandle(this object? obj, GCHandleType type)
    {
        return GCHandle.Alloc(obj, type).ToDisposable(m => m.Free());
    }
}