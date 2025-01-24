namespace FclEx.NewtonsoftJson;

public class ReadSingleAsArrayConverter<T> : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsAssignableTo(typeof(T[]));
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        if (token.Type == JTokenType.Null)
            return null;

        if (token.Type == JTokenType.Array)
        {
            return token.ToObject<T[]>();
        }
        else
        {
            return new[] { token.ToObject<T>() };
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override bool CanRead => true;
    public override bool CanWrite => false;
}