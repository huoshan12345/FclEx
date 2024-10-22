namespace FclEx.Extensions;

public static class ObjectExtensions
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
}