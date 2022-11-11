using System;
using System.Text;
using FclEx.Abp.Serializers;
using FclEx.Extensions;
using Volo.Abp.DependencyInjection;

namespace FclEx.Abp.RabbitMQ.Serializers
{
    public class JsonMemoryBytesSerializer : IMemoryBytesSerializer, ISingletonDependency
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
}
