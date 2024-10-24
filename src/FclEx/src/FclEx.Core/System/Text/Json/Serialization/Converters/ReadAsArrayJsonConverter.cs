namespace System.Text.Json.Serialization.Converters;

/// <summary>
/// A custom JSON converter that reads a single non-array JSON element as an array containing that element.
/// </summary>
/// <remarks>
/// This converter is useful for scenarios where the expected input may be either a single item 
/// or an array of items, allowing for more flexible deserialization. 
/// Note that this converter does not alter the writing behavior; it will not write 
/// a single element as a non-array element.
/// </remarks>
public class ReadAsArrayJsonConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert) => true;

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var elementType = typeToConvert.EnumerableType();
        if (elementType is null)
            throw new InvalidOperationException($"The type to convert '{typeToConvert.ShortName()}' is not a array-like type");

        var token = reader.ReadElement();
        if (token.ValueKind == JsonValueKind.Null)
            return default;

        if (token.ValueKind == JsonValueKind.Array)
        {
            return token.Deserialize(typeToConvert);
        }
        else
        {
            var arrayToken = new JsonArray(token.ToJsonNode());
            return arrayToken.Deserialize(typeToConvert);
        }
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}