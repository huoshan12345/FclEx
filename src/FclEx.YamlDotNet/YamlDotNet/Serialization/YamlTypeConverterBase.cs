namespace YamlDotNet.Serialization;

/// <summary>
/// Provides a typed base implementation for YamlDotNet type converters.
/// </summary>
/// <typeparam name="T">The .NET type handled by the converter.</typeparam>
public abstract class YamlTypeConverterBase<T> : IYamlTypeConverter
{
    /// <summary>
    /// Gets the exact target type accepted by this converter.
    /// </summary>
    public Type TargetType { get; } = typeof(T);

    /// <summary>
    /// Determines whether this converter accepts the specified type.
    /// </summary>
    /// <param name="type">The type being tested by YamlDotNet.</param>
    /// <returns><see langword="true"/> when <paramref name="type"/> exactly matches <typeparamref name="T"/>; otherwise, <see langword="false"/>.</returns>
    public virtual bool Accepts(Type type)
    {
        return type == TargetType;
    }

    /// <summary>
    /// Reads a YAML value as <typeparamref name="T"/>.
    /// </summary>
    /// <param name="parser">The YAML parser positioned at the value to read.</param>
    /// <param name="deserializer">The nested deserializer that can be used for complex values.</param>
    /// <returns>The converted value.</returns>
    public abstract T? ReadYaml(IParser parser, ObjectDeserializer deserializer);

    /// <summary>
    /// Reads a YAML value through the non-generic YamlDotNet converter interface.
    /// </summary>
    /// <param name="parser">The YAML parser positioned at the value to read.</param>
    /// <param name="type">The expected type requested by YamlDotNet.</param>
    /// <param name="deserializer">The nested deserializer that can be used for complex values.</param>
    /// <returns>The converted value.</returns>
    public virtual object? ReadYaml(IParser parser, Type type, ObjectDeserializer deserializer)
    {
        return ReadYaml(parser, deserializer);
    }

    /// <summary>
    /// Writes a value of <typeparamref name="T"/> as YAML.
    /// </summary>
    /// <param name="emitter">The YAML emitter that receives events.</param>
    /// <param name="value">The value to write. Implementations should decide how to represent <see langword="null"/>.</param>
    /// <param name="serializer">The nested serializer that can be used for complex values.</param>
    public abstract void WriteYaml(IEmitter emitter, T? value, ObjectSerializer serializer);

    /// <summary>
    /// Writes a YAML value through the non-generic YamlDotNet converter interface.
    /// </summary>
    /// <param name="emitter">The YAML emitter that receives events.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="type">The runtime type requested by YamlDotNet.</param>
    /// <param name="serializer">The nested serializer that can be used for complex values.</param>
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        WriteYaml(emitter, (T?)value, serializer);
    }
}
