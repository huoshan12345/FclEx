namespace FclEx.Json;

public static class JsonHelper
{
    private static readonly ConcurrentDictionary<JsonOptions, JsonSerializerOptions> _serializerOptions = new();

    public static JsonSerializerOptions GetOptions(JsonOptions options)
    {
        return _serializerOptions.GetOrAdd(options, k => new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = k.PropertyNameCaseInsensitive,
            DefaultIgnoreCondition = k.IgnoreNull ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never,
            WriteIndented = k.Indented,
            PropertyNamingPolicy = k.PropertyNamingPolicy,
        });
    }
}