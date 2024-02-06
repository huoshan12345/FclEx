namespace FclEx.Json;

public class IgnoreJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {

    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return default;
    }

    public override bool CanConvert(Type objectType) => false;
}