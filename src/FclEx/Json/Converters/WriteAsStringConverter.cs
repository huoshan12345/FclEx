using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FclEx.Json.Converters
{
    public class WriteAsStringConverter : ConverterWithDefault<WriteAsStringConverter>
    {
        public override bool CanConvert(Type objectType) => true;
        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var type = value.GetType();
                if (type == typeof(string) || type.IsPrimitive || type.IsNullable() && type.UnwarpNullable().IsPrimitive)
                {
                    writer.WriteValue(value);
                }
                else
                {
                    writer.WriteValue(JsonConvert.SerializeObject(value, DefaultSettings));
                }
            }
        }
    }
}
