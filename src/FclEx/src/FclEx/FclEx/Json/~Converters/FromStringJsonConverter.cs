#pragma warning disable CA2252
namespace FclEx.Json;

public interface IFromString<out T> where T : IFromString<T>
{
    static abstract T FromString(string str);
}

public class FromStringJsonConverter<T> : JsonConverter<T> where T : IFromString<T>
{
    protected readonly Func<string, T> _fromString;

    public FromStringJsonConverter(Func<string, T> fromString)
    {
        _fromString = fromString;
    }

    public FromStringJsonConverter() : this(T.FromString) { }


    public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
    {
        if (value is null)
            writer.WriteNull();
        else
            writer.WriteValue(value.ToString());
    }

    public override T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var tokenType = reader.TokenType;

        if (tokenType == JsonToken.Null)
            return default;

        var token = JToken.Load(reader);

        if (token.Type != JTokenType.String)
            throw new InvalidOperationException($"Expected a string token but got '{token.Type}'");

        var value = token.Value<string>();

        return value.IsNullOrEmpty()
            ? default
            : _fromString(value);
    }
}
