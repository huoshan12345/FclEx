namespace System.Text.Json.Serialization.Converters;

public class StringJsonConverter<T> : JsonConverter<T>
{
    protected readonly Func<string?, T?> _fromString;
    protected readonly Func<T?, string?> _toString;

    public StringJsonConverter(Func<string?, T?> fromString, Func<T?, string?> toString)
    {
        _fromString = fromString;
        _toString = toString;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var tokenType = reader.TokenType;

        if (tokenType != JsonTokenType.String)
            throw new InvalidOperationException($"Expected a string token but got '{tokenType}'");

        var value = reader.GetString();
        return _fromString(value);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var str = _toString(value);
        writer.WriteStringValue(str);
    }
}