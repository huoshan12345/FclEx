using System.Text.Json.Serialization.Converters;

namespace System.Text.Json;

public static class JsonHelper
{
    private static readonly ConcurrentDictionary<JsonOptions, JsonSerializerOptions> _serializerOptions = new();

    public static JsonSerializerOptions GetOptions(JsonOptions options = default)
    {
        return _serializerOptions.GetOrAdd(options, Create);

        static JsonSerializerOptions Create(JsonOptions k)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = k.PropertyNameCaseSensitive,
                DefaultIgnoreCondition = k.IgnoreNull
                    ? JsonIgnoreCondition.WhenWritingNull
                    : JsonIgnoreCondition.Never,
                WriteIndented = k.Indented,
                PropertyNamingPolicy = k.PropertyNamingPolicy,
                Encoder = k.StrictEscaping ? null : RelaxedEncoder.Instance,
                NumberHandling = k.DisallowNumberFromString
                    ? JsonNumberHandling.Strict
                    : JsonNumberHandling.AllowReadingFromString,
            };

            if (k.DisallowBoolFromString == false)
                options.Converters.Add(BooleanJsonConverter.Instance);

            options.MakeReadOnly(true);
            return options;
        }
    }
}