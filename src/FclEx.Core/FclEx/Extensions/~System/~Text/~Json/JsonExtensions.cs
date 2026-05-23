namespace FclEx.Extensions;

public static class JsonExtensions
{
    public static JsonNode? ToJsonNode(this JsonElement element, JsonNodeOptions? options = null)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Array => JsonArray.Create(element, options),
            JsonValueKind.Object => JsonObject.Create(element, options),
            _ => JsonValue.Create(element, options),
        };
    }

    public static JsonElement? ToJsonElement(this JsonNode node, JsonSerializerOptions? options = null)
    {
        return node.Deserialize<JsonElement>(options);
    }

    public static JsonElement? ToJsonElement(this JsonNode node, JsonOptions options)
    {
        return node.ToJsonElement(JsonHelper.GetOptions(options));
    }

    public static string ToJsonString(this JsonNode node, JsonOptions options)
    {
        return node.ToJsonString(JsonHelper.GetOptions(options));
    }

    public static T? ToObject<T>(this JsonNode node, JsonSerializerOptions? options = null)
    {
        return node.Deserialize<T>(options ?? JsonHelper.GetOptions());
    }

    public static T? ToObject<T>(this JsonNode node, JsonOptions options)
    {
        return node.ToObject<T>(JsonHelper.GetOptions(options));
    }

    public static T? ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
    {
        return element.Deserialize<T>(options ?? JsonHelper.GetOptions());
    }

    public static T? ToObject<T>(this JsonElement element, JsonOptions options)
    {
        return element.ToObject<T>(JsonHelper.GetOptions(options));
    }

    public static T? ToObject<T>(this JsonDocument document, JsonSerializerOptions? options = null)
    {
        return document.Deserialize<T>(options ?? JsonHelper.GetOptions());
    }

    public static T? ToObject<T>(this JsonDocument document, JsonOptions options)
    {
        return document.ToObject<T>(JsonHelper.GetOptions(options));
    }

    public static void Deconstruct(this JsonProperty property, out string name, out JsonElement value)
    {
        name = property.Name;
        value = property.Value;
    }
}