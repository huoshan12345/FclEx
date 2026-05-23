namespace System.Text.Json.Serialization;

/// <summary>
/// A custom JSON converter that reads a single non-array JSON element as an array containing that element.
/// </summary>
/// <remarks>
/// This converter is useful for scenarios where the expected input may be either a single item 
/// or an array of items, allowing for more flexible deserialization. 
/// Note that this converter does not alter the writing behavior; it will not write 
/// a single element as an array element.
/// </remarks>
public class ReadAsArrayJsonConverter : JsonConverter<object>
{
    public static readonly ReadAsArrayJsonConverter Instance = new();

    public override bool CanConvert(Type typeToConvert) => true;

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var token = reader.ReadElement();
        var typeInfo = options.GetBuiltInJsonTypeInfo(typeToConvert);

        if (typeToConvert == typeof(string) || typeToConvert.IsEnumerable() == false)
            return token.Deserialize(typeInfo); // typeToConvert is not a collection type, just deserialize it with built-in typeInfo.

        if (token.ValueKind == JsonValueKind.Null)
            return default;

        if (token.ValueKind == JsonValueKind.Array)
        {
            return token.Deserialize(typeInfo);
        }
        else
        {
            var arrayToken = new JsonArray(token.ToJsonNode());
            return arrayToken.Deserialize(typeInfo);
        }
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