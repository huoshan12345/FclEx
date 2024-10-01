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

    public void Deserialize(IXunitSerializationInfo info)
    {
        Value = info.GetValue<string>("_value").FromJson<T>();
    }

    public void Serialize(IXunitSerializationInfo info)
    {
        var json = Value.ToJson();
        info.AddValue("_value", json);
    }
}