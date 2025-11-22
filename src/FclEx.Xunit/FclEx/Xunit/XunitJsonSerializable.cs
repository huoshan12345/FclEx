using System.Text.Json;
using System.Text.Json.Serialization;

namespace FclEx.Xunit;

/// <summary>
/// If some of your theory data can't be "serialized" by xUnit.net,
/// then it cannot be encapsulated into the serialization of a test case
/// which we're required to do for the Visual Studio test runner. <br />
/// see https://github.com/xunit/xunit/issues/1473
/// </summary>
/// <typeparam name="T"></typeparam>
public class XunitJsonSerializable<T> : IXunitSerializable
{
    private static readonly JsonSerializerOptions _options;

    public T? Value { get; private set; }

    public XunitJsonSerializable()
    {
    }

    public XunitJsonSerializable(T? value) => Value = value;

    public virtual void Deserialize(IXunitSerializationInfo info)
    {
        Value = info.GetValue<string>("_json").FromJson<T>(_options);
    }

    public virtual void Serialize(IXunitSerializationInfo info)
    {
        var json = Value.ToJson(_options);
        info.AddValue("_json", json);
    }

    static XunitJsonSerializable()
    {
        var serializerOptions = new JsonSerializerOptions();
        serializerOptions.Converters.Add(new IgnoreTypesJsonConverter(typeof(Delegate)));
        _options = serializerOptions;
    }
}