namespace FclEx.Utils;

/// <summary>
/// Preserves string values verbatim and delegates all other values and target types to another string serializer.
/// </summary>
public sealed class StringPassthroughSerializer : IStringSerializer
{
    private readonly IStringSerializer _fallbackSerializer;

    public StringPassthroughSerializer(IStringSerializer fallbackSerializer)
    {
        _fallbackSerializer = Check.NotNull(fallbackSerializer);
    }

    public string Serialize(object? obj)
    {
        return obj is string value
            ? value
            : _fallbackSerializer.Serialize(obj);
    }

    public object? Deserialize(string data, Type type)
    {
        return type == typeof(string)
            ? data
            : _fallbackSerializer.Deserialize(data, type);
    }
}
