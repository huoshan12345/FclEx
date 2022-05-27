using System;
using System.Text;
using FclEx.Extensions;
using Volo.Abp.DependencyInjection;

namespace FclEx.Abp.Serializers
{
    public class JsonBytesSerializer : IBytesSerializer, ISingletonDependency
    {
        public static JsonBytesSerializer Instance { get; } = new();
        public byte[] Serialize(object? obj) => obj.ToJson().ToBytes(Encoding.UTF8);
        public object? Deserialize(byte[] data, Type type) => data.GetString(Encoding.UTF8).ToJToken().ToObject(type);
    }
}
