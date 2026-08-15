namespace FclEx.Utils;

/// <summary>
/// Provides commonly used serializer compositions.
/// </summary>
public static class SerializerPresets
{
    /// <summary>
    /// Preserves strings verbatim and serializes all other values as JSON text.
    /// </summary>
    public static StringPassthroughSerializer StringOrJson { get; } = new(JsonStringSerializer.Instance);

    /// <summary>
    /// Encodes <see cref="StringOrJson"/> output as UTF-8 bytes.
    /// </summary>
    public static Utf8MemoryBytesSerializer Utf8StringOrJson { get; } = new(StringOrJson);
}
