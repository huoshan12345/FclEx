using System;
using FclEx.Abp.Serializers;

namespace FclEx.Abp.RabbitMQ.Serializers;

public interface IMemoryBytesSerializer : ITypeSerializer<ReadOnlyMemory<byte>>
{
}

public static class MemoryBytesSerializerExtensions
{
    public static T? Deserialize<T>(this IMemoryBytesSerializer serializer, ReadOnlyMemory<byte> data)
    {
        return serializer.Deserialize<T, ReadOnlyMemory<byte>>(data);
    }
}