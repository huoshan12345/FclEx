using YamlDotNet.Core;

namespace YamlDotNet.Serialization;

public abstract class BasicYamlTypeConverter<T> : IYamlTypeConverter
{
    public Type Type { get; } = typeof(T);

    public virtual bool Accepts(Type type)
    {
        return type == Type;
    }

    public abstract T? ReadYaml(IParser parser);

    public virtual object? ReadYaml(IParser parser, Type type)
    {
        return ReadYaml(parser);
    }

    public abstract void WriteYaml(IEmitter emitter, T? value);

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        WriteYaml(emitter, (T?)value);
    }
}