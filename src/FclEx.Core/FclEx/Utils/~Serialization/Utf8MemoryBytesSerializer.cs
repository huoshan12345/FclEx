namespace FclEx.Utils;

/// <summary>
/// Adapts an <see cref="IStringSerializer"/> to UTF-8 encoded memory.
/// </summary>
public sealed class Utf8MemoryBytesSerializer : IMemoryBytesSerializer
{
    private readonly IStringSerializer _stringSerializer;

    public Utf8MemoryBytesSerializer(IStringSerializer stringSerializer)
    {
        _stringSerializer = Check.NotNull(stringSerializer);
    }

    public ReadOnlyMemory<byte> Serialize(object? obj, Type type)
    {
        return _stringSerializer.Serialize(obj, type).ToBytes(Encoding.UTF8);
    }

    public object? Deserialize(ReadOnlyMemory<byte> data, Type type)
    {
        return _stringSerializer.Deserialize(data.Span.GetString(Encoding.UTF8), type);
    }
}
