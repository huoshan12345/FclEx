namespace System.Text.Json.Serialization.Converters;

public class BooleanJsonConverter : JsonConverter<bool>
{
    public static readonly BooleanJsonConverter Instance = new();

    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return reader.TokenType switch
        {
            JsonTokenType.Null => false,
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String => reader.GetString() switch
            {
                "true" => true,
                "false" => false,
                var str => throw new JsonException($"Could not convert string '{str}' to bool."),
            },
            var type => throw new JsonException($"Could not convert token type '{type}' to bool."),
        };
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}