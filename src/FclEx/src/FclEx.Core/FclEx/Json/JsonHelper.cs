namespace FclEx.Json;

public static class JsonHelper
{
    private static readonly ConcurrentDictionary<JsonOptions, JsonSerializerOptions> _serializerOptions = new();

    public static JsonSerializerOptions GetOptions(JsonOptions options)
    {
        return _serializerOptions.GetOrAdd(options, Create);

        static JsonSerializerOptions Create(JsonOptions k)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = k.PropertyNameCaseInsensitive,
                DefaultIgnoreCondition = k.IgnoreNull ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never,
                WriteIndented = k.Indented,
                PropertyNamingPolicy = k.PropertyNamingPolicy,
                Encoder = k.RelaxedEscaping ? RelaxedEncoder.Instance : null,
                NumberHandling = k.NumberHandling,
            };
            options.MakeReadOnly(true);
            return options;
        }
    }
}