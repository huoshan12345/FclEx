namespace FclEx.Xunit;

/// <summary>
/// If some of your theory data can't be "serialized" by xUnit.net,
/// then it cannot be encapsulated into the serialization of a test case
/// which we're required to do for the Visual Studio test runner. <br />
/// see https://github.com/xunit/xunit/issues/1473
/// </summary>
/// <typeparam name="T"></typeparam>
public class XunitSerializable<T> : IXunitSerializable
{
    public T? Value { get; private set; }

    public XunitSerializable() { }

    public XunitSerializable(T? value) => Value = value;

    public virtual void Deserialize(IXunitSerializationInfo info)
    {
        Value = info.GetValue<T>("_value");
    }

    public virtual void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue("_value", Value);
    }

    public override string? ToString() => Value?.ToString();
}

// ReSharper disable once PartialTypeWithSinglePart
public static partial class XunitSerializable
{
    public static XunitSerializable<T> Create<T>(T? value) => new(value);
}