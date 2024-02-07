namespace FclEx.Kafka;

public class JsonDeserializer<T> : IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
            return default!;

        var str = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(str)!;
    }
}