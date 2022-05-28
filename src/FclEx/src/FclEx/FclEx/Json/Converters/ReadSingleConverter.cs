using System;
using FclEx.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FclEx.Json.Converters;

public abstract class ReadSingleConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => true;
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    protected abstract Func<JArray, JToken?> SingleFunc { get; }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) 
            return null;

        JToken? token = JToken.ReadFrom(reader);
        if (token.Type == JTokenType.Array)
        {
            token = SingleFunc(token.ToJArray()!);
        }
        return token?.ToObject(objectType);
    }
}