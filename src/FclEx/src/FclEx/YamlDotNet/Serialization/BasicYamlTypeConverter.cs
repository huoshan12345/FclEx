using YamlDotNet.Core;

namespace YamlDotNet.Serialization;

public abstract class BasicYamlTypeConverter<T> : IYamlTypeConverter
{
    public Type Type { get; } = typeof(T);

    public virtual bool Accepts(Type type)
    {
        return type == Type;
    }

    public abstract T? ReadYaml(IParser parser, ObjectDeserializer deserializer);

    public virtual object? ReadYaml(IParser parser, Type type, ObjectDeserializer deserializer)
    {
        return ReadYaml(parser, deserializer);
    }

    public abstract void WriteYaml(IEmitter emitter, T? value, ObjectSerializer serializer);

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        WriteYaml(emitter, (T?)value, serializer);
    }
}