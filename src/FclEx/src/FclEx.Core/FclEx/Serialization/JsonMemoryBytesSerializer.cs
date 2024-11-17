namespace FclEx.Serialization;

public class JsonMemoryBytesSerializer(IStringSerializer StringSerializer) : IMemoryBytesSerializer
{
    public static readonly JsonMemoryBytesSerializer Instance = new(StringAsRawSerializer.Instance);

    public ReadOnlyMemory<byte> Serialize(object? obj) => StringSerializer.Serialize(obj).ToBytes(Encoding.UTF8);

    public object? Deserialize(ReadOnlyMemory<byte> data, Type type)
    {
        var str = data.Span.GetString();
        return StringSerializer.Deserialize(str, type);
    }
}