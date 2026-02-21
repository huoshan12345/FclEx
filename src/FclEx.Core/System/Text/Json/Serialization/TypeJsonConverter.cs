namespace System.Text.Json.Serialization;

public class TypeJsonConverter : JsonConverter<Type>
{
    public static readonly TypeJsonConverter Instance = new();

    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Expected a string token but got {reader.TokenType}");

        // Read the type name from the JSON string
        var typeName = reader.GetString();

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (typeName == null)
            return null;

        // Use Type.GetType to find the type from its assembly-qualified name
        // The 'true, true' arguments handle case sensitivity and error throwing
        return Type.GetType(typeName, true, true);
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        // Write the AssemblyQualifiedName of the type to the JSON
        writer.WriteStringValue(value.AssemblyQualifiedName);
    }
}
