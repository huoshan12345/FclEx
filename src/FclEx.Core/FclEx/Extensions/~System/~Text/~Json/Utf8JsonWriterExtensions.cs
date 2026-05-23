namespace FclEx.Extensions;

public static class Utf8JsonWriterExtensions
{
    public static void Write(this Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
