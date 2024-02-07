namespace FclEx.Serializers;

public class StringAsRawSerializer : IStringSerializer
{
    private readonly IStringSerializer _serializer;

    public StringAsRawSerializer(IStringSerializer serializer)
    {
        _serializer = serializer is StringAsRawSerializer stringAsRaw
            ? stringAsRaw._serializer
            : serializer;
    }

    public object? Deserialize(string data, Type type)
    {
        return type == typeof(string)
            ? data
            : _serializer.Deserialize(data, type);
    }

    public string Serialize(object? obj)
    {
        return obj as string ?? _serializer.Serialize(obj);
    }

    public static StringAsRawSerializer Instance { get; } = new(JsonStringSerializer.Instance);
}