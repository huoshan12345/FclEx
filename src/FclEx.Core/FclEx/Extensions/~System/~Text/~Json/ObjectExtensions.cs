namespace FclEx.Extensions;

public static partial class ObjectExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToJson<T>(this T obj, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(obj, options ?? JsonHelper.GetOptions());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToJson<T>(this T obj, JsonOptions options)
    {
        return obj.ToJson(JsonHelper.GetOptions(options));
    }
}