#if NET6_0_OR_GREATER
namespace YamlDotNet.Serialization;

/// <summary>
/// Converts scalar YAML values to and from types that implement <see cref="INameIdentifier{T}"/>.
/// </summary>
/// <typeparam name="T">The reference type that represents a name-based identifier.</typeparam>
public class NameIdentifierYamlConverter<T> : YamlTypeConverterBase<T> where T : class, INameIdentifier<T>
{
    /// <summary>
    /// Reads a scalar value and converts it with <see cref="INameIdentifier{T}.Create"/>.
    /// </summary>
    /// <param name="parser">The YAML parser positioned at a scalar event.</param>
    /// <param name="deserializer">The nested deserializer. This converter does not use it.</param>
    /// <returns><c>null</c> for null or empty scalar values; otherwise, a name identifier created from the scalar value.</returns>
    public override T? ReadYaml(IParser parser, ObjectDeserializer deserializer)
    {
        var value = parser.Consume<Scalar>().Value;
        return value.IsNullOrEmpty()
            ? null
            : T.Create(value);
    }

    /// <summary>
    /// Writes the identifier's <see cref="INameIdentifier{T}.Name"/> as a scalar value.
    /// </summary>
    /// <param name="emitter">The YAML emitter that receives events.</param>
    /// <param name="value">The value to write. <c>null</c> is emitted as a null scalar.</param>
    /// <param name="serializer">The nested serializer. This converter does not use it.</param>
    public override void WriteYaml(IEmitter emitter, T? value, ObjectSerializer serializer)
    {
        if (value is null)
        {
            emitter.Emit(new Scalar(null!));
            return;
        }

        emitter.Emit(value.Name);
    }
}
#endif
