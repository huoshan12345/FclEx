namespace FclEx.Kafka;

public class NewtonsoftJsonDeserializer<T> : IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
            return default!;

        var str = Encoding.UTF8.GetString(data);
        return str.FromJson<T>()!;
    }
}