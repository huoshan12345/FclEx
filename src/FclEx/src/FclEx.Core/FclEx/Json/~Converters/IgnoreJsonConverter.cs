namespace FclEx.Json;

public class IgnoreJsonConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert) => true;

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return default;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (writer.CurrentDepth == 0)
            return;

        writer.WriteStringValue(default(string));
    }
}