namespace FclEx.Serialization;

public class JsonStringSerializer : IStringSerializer
{
    public static readonly JsonStringSerializer Instance = new();

    public string Serialize(object? obj) => obj.ToJson();
    public object? Deserialize(string str, Type type)
    {
        if (!str.IsPossibleJson())
            throw new InvalidOperationException("Not a valid json string: " + str.Truncate(100));
        return str.FromJson(type);
    }
}