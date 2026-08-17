namespace System.Text.Json.Serialization;

/// <summary>
/// Reads a single JSON value as a one-element collection while delegating normal serialization behavior to the
/// built-in converter for the target type.
/// </summary>
/// <remarks>
/// Unlike <see cref="ReadAsArrayJsonConverter" />, this converter can be applied to a type with
/// <see cref="JsonConverterAttribute" /> because it obtains that type's built-in converter directly. It consequently
/// depends on private System.Text.Json implementation details through <c>GetBuiltInJsonTypeInfo</c> and intentionally
/// bypasses other custom converters selected for the same target type.
/// </remarks>
public sealed class ReadAsArrayUsingBuiltInJsonConverter : JsonConverter<object>
{
    public static readonly ReadAsArrayUsingBuiltInJsonConverter Instance = new();

    public override bool CanConvert(Type typeToConvert) => true;

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var token = reader.ReadElement();
        var typeInfo = options.GetBuiltInJsonTypeInfo(typeToConvert);

        if (typeToConvert == typeof(string) || typeToConvert.IsEnumerable() == false)
            return token.Deserialize(typeInfo);

        if (token.ValueKind is JsonValueKind.Null or JsonValueKind.Array)
            return token.Deserialize(typeInfo);

        var arrayToken = new JsonArray(token.ToJsonNode());
        return arrayToken.Deserialize(typeInfo);
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var typeInfo = options.GetBuiltInJsonTypeInfo(value.GetType());
        JsonSerializer.Serialize(writer, value, typeInfo);
    }
}