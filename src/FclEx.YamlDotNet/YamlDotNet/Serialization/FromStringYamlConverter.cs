#if NET6_0_OR_GREATER
namespace YamlDotNet.Serialization;

public class FromStringYamlConverter<T> : YamlTypeConverterBase<T> where T : class, IFromString<T>
{
    public override T? ReadYaml(IParser parser, ObjectDeserializer deserializer)
    {
        var value = parser.Consume<Scalar>().Value;
        return value.IsNullOrEmpty()
            ? null
            : T.FromString(value);
    }
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
