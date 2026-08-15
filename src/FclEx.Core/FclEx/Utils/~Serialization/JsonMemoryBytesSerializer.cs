namespace FclEx.Utils;

public class JsonMemoryBytesSerializer(IStringSerializer stringSerializer) : IMemoryBytesSerializer
{
    public static readonly JsonMemoryBytesSerializer Instance = new(StringAsRawSerializer.Instance);

    public ReadOnlyMemory<byte> Serialize(object? obj)
        => stringSerializer.Serialize(obj).ToBytes(Encoding.UTF8);

    public object? Deserialize(ReadOnlyMemory<byte> data, Type type)
    {
        var str = data.Span.GetString();
        return stringSerializer.Deserialize(str, type);
    }
}