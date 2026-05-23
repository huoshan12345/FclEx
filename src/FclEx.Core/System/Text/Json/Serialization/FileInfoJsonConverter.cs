namespace System.Text.Json.Serialization;

public sealed class FileInfoJsonConverter : JsonConverter<FileInfo>
{
    public static readonly FileInfoJsonConverter Instance = new();

    public override FileInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Expected string, got {reader.TokenType}");

        var path = reader.GetString();

        if (string.IsNullOrWhiteSpace(path))
            return null;

        return new FileInfo(path);
    }

    public override void Write(Utf8JsonWriter writer, FileInfo? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.FullName);
    }
}
