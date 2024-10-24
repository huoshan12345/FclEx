namespace FclEx.Serialization;

public static class Extensions
{
    public static T? Deserialize<T, TTarget>(this ITypeSerializer<TTarget> serializer, TTarget data)
    {
        return serializer.Deserialize(data, typeof(T)).CastTo<T>();
    }

    public static T? Deserialize<T>(this IStringSerializer serializer, string data)
    {
        return serializer.Deserialize<T, string>(data);
    }

    public static T? Deserialize<T>(this IBytesSerializer serializer, byte[] data)
    {
        return serializer.Deserialize<T, byte[]>(data);
    }
}