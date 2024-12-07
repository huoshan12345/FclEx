namespace FclEx.Json;

public static class JsonHelper
{
    private static readonly ConcurrentDictionary<JsonOptions, JsonSerializerOptions> _serializerOptions = new();

    private static readonly DefaultJsonTypeInfoResolver Resolver = new() { Modifiers = { EmptyValueModifier } };

    public static JsonSerializerOptions GetOptions(JsonOptions options = default)
    {
        return _serializerOptions.GetOrAdd(options, Create);

        static JsonSerializerOptions Create(JsonOptions k)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = k.PropertyNameCaseSensitive == false,
                DefaultIgnoreCondition = k.IgnoreNull
                    ? JsonIgnoreCondition.WhenWritingNull
                    : JsonIgnoreCondition.Never,
                WriteIndented = k.Indented,
                PropertyNamingPolicy = k.PropertyNamingPolicy,
                Encoder = k.StrictEscaping ? null : RelaxedEncoder.Instance,
                NumberHandling = k.DisallowNumberFromString
                    ? JsonNumberHandling.Strict
                    : JsonNumberHandling.AllowReadingFromString,
                TypeInfoResolver = Resolver,
            };

            if (k.DisallowBoolFromString == false)
                options.Converters.Add(BooleanJsonConverter.Instance);

            options.MakeReadOnly(true);
            return options;
        }
    }

    public static void EmptyValueModifier(JsonTypeInfo typeInfo)
    {
        foreach (var property in typeInfo.Properties)
        {
            var type = property.PropertyType;
            if (type.IsEnumerable() && property.AttributeProvider?.IsDefined<JsonIgnoreEmptyAttribute>(true) != null)
            {
                property.ShouldSerialize = (_, val) => ((IEnumerable?)val).IsNullOrEmpty() == false;
            }
        }
    }
}