namespace FclEx.Extensions;

public static partial class ObjectExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static string ToJson<T>(this T obj, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(obj, options ?? JsonHelper.GetOptions());
    }

    [MethodImpl(AggressiveInlining)]
    public static string ToJson(this object? obj, Type inputType, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(obj, inputType, options ?? JsonHelper.GetOptions());
    }

    [MethodImpl(AggressiveInlining)]
    public static string ToJson<T>(this T obj, JsonOptions options)
    {
        return obj.ToJson(JsonHelper.GetOptions(options));
    }

    [MethodImpl(AggressiveInlining)]
    public static string ToJson(this object? obj, Type inputType, JsonOptions options)
    {
        return obj.ToJson(inputType, JsonHelper.GetOptions(options));
    }
}