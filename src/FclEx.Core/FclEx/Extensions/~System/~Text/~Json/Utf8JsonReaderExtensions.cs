namespace FclEx.Extensions;

public static class Utf8JsonReaderExtensions
{
    public static JsonNode? ReadNode(ref this Utf8JsonReader reader, JsonNodeOptions? options = null)
    {
        return JsonNode.Parse(ref reader, options);
    }

    public static JsonNode? ReadNode(ref this Utf8JsonReader reader, JsonSerializerOptions options)
    {
        return JsonNode.Parse(ref reader, new() { PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive });
    }

    public static JsonElement ReadElement(ref this Utf8JsonReader reader)
    {
        return JsonElement.ParseValue(ref reader);
    }
}