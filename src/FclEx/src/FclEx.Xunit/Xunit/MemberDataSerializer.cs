using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Xunit;

/// <summary>
/// If some of your theory data can't be "serialized" by xUnit.net, 
/// then it cannot be encapsulated into the serialization of a test case
/// which we're required to do for the Visual Studio test runner. <br/>
/// see https://github.com/xunit/xunit/issues/1473
/// </summary>
/// <typeparam name="T"></typeparam>
public class MemberDataSerializer<T> : IXunitSerializable
{
    public T? Value { get; private set; }

    // required for deserializer
    public MemberDataSerializer() { }

    public MemberDataSerializer(T value)
    {
        Value = value;
    }

    private static Action<JsonTypeInfo> RemoveDelegate()
    {
        return m =>
        {
            if (m.Kind == JsonTypeInfoKind.Object)
            {
                m.Properties.RemoveAll(m => m.PropertyType.IsAssignableTo(typeof(Delegate)));
            }
        };
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { RemoveDelegate() },
        },
    };

    public virtual void Deserialize(IXunitSerializationInfo info)
    {
        Value = info.GetValue<string>("_value").FromJson<T>(_options);
    }


    public virtual void Serialize(IXunitSerializationInfo info)
    {
        var json = Value.ToJson(_options);
        info.AddValue("_value", json);
    }
}