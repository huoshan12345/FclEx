namespace System.Text.Json.Serialization;

public class IgnoreJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => true;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var type = typeof(IgnoreJsonConverterImpl<>).MakeGenericType(typeToConvert);
        return type.CreateObject<JsonConverter>();
    }
}

public class IgnoreJsonConverterImpl<T> : JsonConverter<T>
{
    public override bool CanConvert(Type typeToConvert) => true;

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (writer.CurrentDepth == 0)
            return;

        writer.WriteNullValue();
    }
}