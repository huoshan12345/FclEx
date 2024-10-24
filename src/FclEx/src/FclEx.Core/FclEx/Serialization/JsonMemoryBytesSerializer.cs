namespace FclEx.Serialization;

public class JsonMemoryBytesSerializer : IMemoryBytesSerializer
{
    private readonly IStringSerializer _stringSerializer;

    public JsonMemoryBytesSerializer(IStringSerializer stringSerializer)
    {
        _stringSerializer = stringSerializer;
    }

    public static JsonMemoryBytesSerializer Instance { get; } = new(StringAsRawSerializer.Instance);

    public ReadOnlyMemory<byte> Serialize(object? obj) => _stringSerializer.Serialize(obj).ToBytes(Encoding.UTF8);

    public object? Deserialize(ReadOnlyMemory<byte> data, Type type)
    {
        var str = data.Span.GetString();
        return _stringSerializer.Deserialize(str, type);
    }
}