namespace FclEx.Utils;

public interface IMemoryBytesSerializer : ITypeSerializer<ReadOnlyMemory<byte>>;

public static class MemoryBytesSerializerExtensions
{
    public static T? Deserialize<T>(this IMemoryBytesSerializer serializer, ReadOnlyMemory<byte> data)
    {
        return serializer.Deserialize<T, ReadOnlyMemory<byte>>(data);
    }
}