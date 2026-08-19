namespace FclEx.Extensions;

public static class JsonElementExtensions
{
    public static bool HasProperty(this JsonElement element, string name)
    {
        return element.TryGetProperty(name, out _);
    }

    public static JsonNode? ToJsonNode(this JsonElement element, JsonNodeOptions? options = null)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Array => JsonArray.Create(element, options),
            JsonValueKind.Object => JsonObject.Create(element, options),
            _ => JsonValue.Create(element, options),
        };
    }

    public static T? ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
    {
        return element.Deserialize<T>(options ?? JsonHelper.GetOptions());
    }

    public static T? ToObject<T>(this JsonElement element, JsonOptions options)
    {
        return element.ToObject<T>(JsonHelper.GetOptions(options));
    }
    
    extension(JsonElement)
    {
        public static JsonElement From<T>(T obj, JsonSerializerOptions? options = null)
        {
            options ??= JsonHelper.GetOptions();
            return JsonSerializer.SerializeToElement(obj, options);
        }
    }
}