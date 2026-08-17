namespace FclEx.Utils;

public class JsonBytesSerializer : IBytesSerializer
{
    public static JsonBytesSerializer Instance { get; } = new();
    public byte[] Serialize(object? obj, Type type) => obj.ToJson(type).ToBytes(Encoding.UTF8);
    public object? Deserialize(byte[] data, Type type) => data.GetString(Encoding.UTF8).FromJson(type);
}