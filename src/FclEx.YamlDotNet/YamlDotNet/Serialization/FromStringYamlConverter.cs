#if NET6_0_OR_GREATER
namespace YamlDotNet.Serialization;

/// <summary>
/// Converts scalar YAML values to and from types that implement <see cref="IFromString{T}"/>.
/// </summary>
/// <typeparam name="T">The reference type that can create itself from a string.</typeparam>
public class FromStringYamlConverter<T> : YamlTypeConverterBase<T> where T : class, IFromString<T>
{
    /// <summary>
    /// Reads a scalar value and converts it with <see cref="IFromString{T}.FromString"/>.
    /// </summary>
    /// <param name="parser">The YAML parser positioned at a scalar event.</param>
    /// <param name="deserializer">The nested deserializer. This converter does not use it.</param>
    /// <returns><see langword="null"/> for null or empty scalar values; otherwise, the value returned by <see cref="IFromString{T}.FromString"/>.</returns>
    public override T? ReadYaml(IParser parser, ObjectDeserializer deserializer)
    {
        var value = parser.Consume<Scalar>().Value;
        return value.IsNullOrEmpty()
            ? null
            : T.FromString(value);
    }

    /// <summary>
    /// Writes the value using its <see cref="object.ToString"/> result.
    /// </summary>
    /// <param name="emitter">The YAML emitter that receives events.</param>
    /// <param name="value">The value to write. <see langword="null"/> is emitted as a null scalar.</param>
    /// <param name="serializer">The nested serializer. This converter does not use it.</param>
    /// <remarks>If <see cref="object.ToString"/> returns <see langword="null"/>, a null scalar is emitted.</remarks>
    public override void WriteYaml(IEmitter emitter, T? value, ObjectSerializer serializer)
    {
        if (value is null)
        {
            emitter.Emit(new Scalar(null!));
            return;
        }

        var str = value.ToString();

        if (str is null)
        {
            emitter.Emit(new Scalar(null!));
            return;
        }

        emitter.Emit(str);
    }
}
#endif
