namespace FclEx.Utils;

public class JsonStringSerializer : IStringSerializer
{
    public static readonly JsonStringSerializer Instance = new();

    public string Serialize(object? obj, Type type) => obj.ToJson(type);
    public object? Deserialize(string str, Type type) => str.FromJson(type);
}