namespace FclEx.Utils;

public class JsonStringSerializer : IStringSerializer
{
    public static readonly JsonStringSerializer Instance = new();

    public string Serialize(object? obj) => obj.ToJson();
    public object? Deserialize(string str, Type type) => str.FromJson(type);
}