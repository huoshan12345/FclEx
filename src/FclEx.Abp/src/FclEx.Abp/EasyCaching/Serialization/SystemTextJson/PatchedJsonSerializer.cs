using System;
using System.IO;
using System.Text.Encodings.Web;

namespace EasyCaching.Serialization.SystemTextJson;

public class PatchedJsonSerializer : IEasyCachingSerializer
{
    private readonly JsonSerializerOptions _options;
    private readonly DefaultJsonSerializer _serializer;
    private readonly JsonWriterOptions _jsonWriterOption;

    public PatchedJsonSerializer(string name, JsonSerializerOptions options)
    {
        _options = options;
        _serializer = new(name, options);
        _jsonWriterOption = new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    }

    public byte[] Serialize<T>(T value) => _serializer.Serialize(value);

    public T Deserialize<T>(byte[] bytes) => _serializer.Deserialize<T>(bytes);

    public object Deserialize(byte[] bytes, Type type) => _serializer.Deserialize(bytes, type);

    public ArraySegment<byte> SerializeObject(object obj)
    {
        var str = EasyCaching.Core.Internal.TypeHelper.BuildTypeName(obj.GetType());
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, _jsonWriterOption);
        writer.WriteStartArray();
        writer.WriteStringValue(str);
        JsonSerializer.Serialize(writer, obj, _options);
        writer.WriteEndArray();
        writer.Flush();
        return stream.ToArray().ToSegment();
    }

    public object DeserializeObject(ArraySegment<byte> value) => _serializer.DeserializeObject(value);

    public string Name => _serializer.Name;
}