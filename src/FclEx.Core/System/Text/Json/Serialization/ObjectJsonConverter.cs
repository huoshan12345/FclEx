namespace System.Text.Json.Serialization;

public class ObjectJsonConverter : JsonConverter<object>
{
    public static readonly ObjectJsonConverter Instance = new();

    private const int MaxDepth = 64;

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadValue(ref reader, options, 0);
    }

    private static object? ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options, int depth)
    {
        if (depth > MaxDepth)
            throw new JsonException($"Maximum JSON depth of {MaxDepth} exceeded.");

        return reader.TokenType switch
        {
            JsonTokenType.Number => ReadNumber(ref reader),
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Null => null,
            JsonTokenType.StartObject => ReadObject(ref reader, options, depth),
            JsonTokenType.StartArray => ReadArray(ref reader, options, depth),
            _ => JsonElement.ParseValue(ref reader)
        };

        static object ReadNumber(ref Utf8JsonReader reader)
        {
            if (reader.TryGetInt32(out var i))
                return i;

            if (reader.TryGetInt64(out var l))
                return l;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (reader.TryGetDouble(out var d))
                return d;

            throw new JsonException("Invalid JSON number.");
        }

        static object ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options, int depth)
        {
            var dict = new Dictionary<string, object?>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return dict;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException($"Expected {JsonTokenType.PropertyName} but found {reader.TokenType} while reading an object.");

                var propName = reader.GetString()
                          ?? throw new JsonException("Object property name cannot be null.");

                reader.Read();

                dict[propName] = ReadValue(ref reader, options, depth + 1);
            }

            throw new JsonException("Unexpected end of JSON while reading an object.");
        }

        static object ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options, int depth)
        {
            var list = new List<object?>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return list;

                var item = ReadValue(ref reader, options, depth + 1);
                list.Add(item);
            }

            throw new JsonException("Unexpected end of JSON while reading an array.");
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
