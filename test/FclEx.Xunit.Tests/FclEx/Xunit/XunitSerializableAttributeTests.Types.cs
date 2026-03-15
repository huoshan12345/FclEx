namespace FclEx.Xunit;

partial class XunitSerializableAttributeTests
{
    [XunitSerializable]
    public partial record TestModel<T>(Type? Type, object? Value, bool Valid = false)
    {
        public string Member { get; } = Type is null ? "" : Type.Name;
    }

    [Fact]
    public void Should_RoundTrip_With_Type()
    {
        var original = new TestModel<string>(typeof(string), 1);
        var info = CreateSerializationInfo();
        original.Serialize(info);

        var deserialized = new TestModel<string>(null, null);
        deserialized.Deserialize(info);

        Assert.Equal(original.Type, deserialized.Type);
        Assert.Equal(original.Value, deserialized.Value);
        Assert.Equal(original.Valid, deserialized.Valid);
    }
}
