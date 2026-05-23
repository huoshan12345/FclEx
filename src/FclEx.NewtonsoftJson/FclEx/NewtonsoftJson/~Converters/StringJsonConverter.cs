namespace FclEx.NewtonsoftJson;

public class StringJsonConverter<T> : JsonConverter<T>
{
    protected readonly Func<string?, T?> _fromString;
    protected readonly Func<T?, string?> _toString;

    public StringJsonConverter(Func<string?, T?> fromString, Func<T?, string?> toString)
    {
        _fromString = fromString;
        _toString = toString;
    }

    public override T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var tokenType = reader.TokenType;

        if (tokenType == JsonToken.Null)
            return _fromString(null);

        var token = JToken.Load(reader);

        if (token.Type != JTokenType.String)
            throw new InvalidOperationException($"Expected a string token but got '{token.Type}'");

        var value = token.Value<string>();
        return _fromString(value);
    }

    public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
    {
        var str = _toString(value);

        if (str is null)
            writer.WriteNull();
        else
            writer.WriteValue(str);
    }
}
