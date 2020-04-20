using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FclEx.Json.Converters
{
    public class ReadAsStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) 
                return null;

            var token = JToken.ReadFrom(reader);
            return token.Type == JTokenType.String 
                ? token.Value<string>()
                : token.ToString(Formatting.None);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
