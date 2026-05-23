#if NET6_0_OR_GREATER
namespace YamlDotNet.Serialization;

public class FromToStringYamlConverter<T> : BasicYamlTypeConverter<T> where T : class, IFromString<T>
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
            return;

        var str = value.ToString();

        if (str is null)
            return;

        emitter.Emit(str);
    }
}
#endif