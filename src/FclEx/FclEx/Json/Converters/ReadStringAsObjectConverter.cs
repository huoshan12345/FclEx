using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FclEx.Json.Converters
{
    public class ReadStringAsObjectConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) 
                return null;

            var token = JToken.ReadFrom(reader);
            if (objectType == typeof(string))
            {
                return token.Value<string>();
            }
            if (token.Type == JTokenType.String)
            {
                var str = token.Value<string>();
                token = str?.ToJToken();
            }
            return token?.ToObject(objectType);
        }

        public override bool CanConvert(Type objectType) => true;
        public override bool CanRead => true;
        public override bool CanWrite => false;
    }
}
