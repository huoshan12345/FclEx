#if NET6_0_OR_GREATER
namespace YamlDotNet.Serialization;

public class NameIdentifierYamlConverter<T> : YamlTypeConverterBase<T> where T : class, INameIdentifier<T>
{
    public override T? ReadYaml(IParser parser, ObjectDeserializer deserializer)
    {
        var value = parser.Consume<Scalar>().Value;
        return value.IsNullOrEmpty()
            ? null
            : T.Create(value);
    }

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
