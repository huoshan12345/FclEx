namespace FclEx.Utils;

public static class SerializationExtensions
{
    public static TTarget Serialize<T, TTarget>(this ITypeSerializer<TTarget> serializer, T obj)
    {
        return serializer.Serialize(obj, typeof(T));
    }

    public static T? Deserialize<T, TTarget>(this ITypeSerializer<TTarget> serializer, TTarget data)
    {
        return serializer.Deserialize(data, typeof(T)).CastTo<T>();
    }

    public static string Serialize<T>(this IStringSerializer serializer, T obj)
    {
        return serializer.Serialize<T, string>(obj);
    }

    public static T? Deserialize<T>(this IStringSerializer serializer, string data)
    {
        return serializer.Deserialize<T, string>(data);
    }

    public static byte[] Serialize<T>(this IBytesSerializer serializer, T obj)
    {
        return serializer.Serialize<T, byte[]>(obj);
    }

    public static T? Deserialize<T>(this IBytesSerializer serializer, byte[] data)
    {
        return serializer.Deserialize<T, byte[]>(data);
    }
}