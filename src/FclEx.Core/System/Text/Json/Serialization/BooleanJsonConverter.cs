namespace System.Text.Json.Serialization;

public class BooleanJsonConverter : JsonConverter<bool>
{
    public static readonly BooleanJsonConverter Strict = new(false);
    public static readonly BooleanJsonConverter NullAsFalse = new(true);

    public BooleanJsonConverter()
        : this(false)
    {
    }

    public BooleanJsonConverter(bool treatNullAsFalse)
    {
        TreatNullAsFalse = treatNullAsFalse;
    }

    public bool TreatNullAsFalse { get; }

    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return reader.TokenType switch
        {
            JsonTokenType.Null when TreatNullAsFalse => false,
            JsonTokenType.Null => throw new JsonException("Could not convert null to bool."),
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String => ParseString(reader.GetString()),
            var type => throw new JsonException($"Could not convert token type '{type}' to bool."),
        };
    }

    private static bool ParseString(string? value)
    {
        return bool.TryParse(value, out var result)
            ? result
            : throw new JsonException($"Could not convert string '{value}' to bool.");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}
