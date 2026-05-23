namespace FclEx.NewtonsoftJson;

public class WriteAsStringConverter : BypassSelfJsonConverter
{
    public override bool CanConvert(Type objectType) => true;
    public override bool CanRead => false;
    public override bool CanWrite => true;

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
        }
        else
        {
            var type = value.GetType().UnwrapNullable();
            if (type == typeof(string))
            {
                writer.WriteValue((string)value);
            }
            else if (type.IsPrimitive)
            {
                writer.WriteValue(value.ToString());
            }
            else
            {
                var str = JsonConvert.SerializeObject(value, BypassSettings);
                writer.WriteValue(str);
            }
        }
    }
}