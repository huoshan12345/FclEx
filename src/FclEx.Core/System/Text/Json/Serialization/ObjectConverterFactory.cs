namespace System.Text.Json.Serialization;

public sealed class ObjectConverterFactory : JsonConverterFactory
{
    public static readonly ObjectConverterFactory Instance = new();

    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == typeof(object))
            return true;

        if (typeToConvert == typeof(Dictionary<string, object>))
            return true;

        if (typeToConvert == typeof(List<object>))
            return true;

        return false;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return ObjectJsonConverter.Instance;
    }
}