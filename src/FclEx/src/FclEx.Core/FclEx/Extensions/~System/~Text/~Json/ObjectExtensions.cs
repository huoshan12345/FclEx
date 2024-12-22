namespace FclEx.Extensions;

public static partial class ObjectExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToJson(this object? obj, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(obj, options ?? JsonHelper.GetOptions());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToJson(this object? obj, JsonOptions options)
    {
        return obj.ToJson(JsonHelper.GetOptions(options));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToJsonCamelCase(this object? obj)
    {
        return obj.ToJson(new JsonOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsonNode? ToJsonNode<T>(this T? value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.SerializeToNode(value, options ?? JsonHelper.GetOptions());
    }
}