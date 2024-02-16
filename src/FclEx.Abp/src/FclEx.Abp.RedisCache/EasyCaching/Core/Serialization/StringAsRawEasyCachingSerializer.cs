using System;
using System.Text;
using FclEx.Extensions;

namespace EasyCaching.Core.Serialization;

public sealed class StringAsRawEasyCachingSerializer : IEasyCachingSerializer
{
    public Encoding Encoding { get; }
    public IEasyCachingSerializer Serializer { get; }

    public StringAsRawEasyCachingSerializer(IEasyCachingSerializer serializer, Encoding? encoding = null, string? name = null)
    {
        Serializer = GetRawSerializer(serializer);
        Encoding = encoding ?? Encoding.UTF8;
        Name = (name, DefaultName).FirstNotEmpty();
    }

    private static IEasyCachingSerializer GetRawSerializer(IEasyCachingSerializer serializer)
    {
        var p = serializer;
        while (p is StringAsRawEasyCachingSerializer stringAsRawSerializer)
        {
            p = stringAsRawSerializer.Serializer;
        }
        return p;
    }

    public byte[] Serialize<T>(T value)
    {
        return typeof(T) == typeof(string)
            ? Encoding.GetBytes(value.CastTo<string>()!)
            : Serializer.Serialize(value);
    }

    public T Deserialize<T>(byte[] bytes)
    {
        return typeof(T) == typeof(string)
            ? Encoding.GetString(bytes).CastTo<T>()
            : Serializer.Deserialize<T>(bytes);
    }

    public object Deserialize(byte[] bytes, Type type)
    {
        return type == typeof(string)
            ? Encoding.GetString(bytes)
            : Serializer.Deserialize(bytes, type);
    }

    public ArraySegment<byte> SerializeObject(object obj)
    {
        return Serializer.SerializeObject(obj);
    }

    public object DeserializeObject(ArraySegment<byte> value)
    {
        return Serializer.DeserializeObject(value);
    }

    public string Name { get; }

    public const string DefaultName = "stringasraw";
}