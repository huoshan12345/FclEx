namespace System.Text.Json.Serialization;

/// <summary>
/// Consumes JSON values while reading and writes nested values as JSON <see langword="null"/>.
/// </summary>
/// <remarks>
/// This converter is intended for placeholder types whose payload should be ignored, such as <see cref="FclEx.Unit"/>.<br/>
/// It cannot omit a property once <see cref="JsonSerializer"/> has decided to write it; for example,
/// <see cref="JsonIgnoreCondition.WhenWritingNull"/> checks the original CLR value, not the <see langword="null"/>
/// emitted by this converter. Use <see cref="JsonIgnoreAttribute"/> when a property should be omitted entirely.
/// </remarks>
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
        reader.Skip();
        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (writer.CurrentDepth == 0)
            return;

        writer.WriteNullValue();
    }
}
