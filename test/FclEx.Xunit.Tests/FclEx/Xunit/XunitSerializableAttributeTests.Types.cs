using System.Text.Json.Serialization;

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

    [XunitSerializable]
    public partial record TestModel2<T>([field: JsonIgnore] Expression<Func<T, object>>? Selector, Type? Type, object? Value, bool Valid = false)
    {
        public string Member { get; } = Type is null ? "" : Type.Name;
    }

    [Fact]
    public void Should_RoundTrip_With_JsonIgnore()
    {
        var original = new TestModel2<string>(m => m.Length, typeof(string), 1);
        var info = CreateSerializationInfo();
        original.Serialize(info);

        var deserialized = new TestModel2<string>(null, null, null);
        deserialized.Deserialize(info);

#if FCLEX_XUNIT_V3
        Assert.Null(deserialized.Selector);
#else
        Assert.Equal(original.Selector, deserialized.Selector);
#endif

        Assert.Equal(original.Type, deserialized.Type);
        Assert.Equal(original.Value, deserialized.Value);
        Assert.Equal(original.Valid, deserialized.Valid);
    }

    [XunitSerializable]
    public partial record TestModel3<T>([field: JsonConverter(typeof(IgnoreJsonConverter))] Expression<Func<T, object>>? Selector, Type? Type, object? Value, bool Valid = false)
    {
        public string Member { get; } = Type is null ? "" : Type.Name;
    }

    [Fact]
    public void Should_RoundTrip_With_JsonConverter()
    {
        var original = new TestModel2<string>(m => m.Length, typeof(string), 1);
        var info = CreateSerializationInfo();
        original.Serialize(info);

        var deserialized = new TestModel2<string>(null, null, null);
        deserialized.Deserialize(info);

#if FCLEX_XUNIT_V3
        Assert.Null(deserialized.Selector);
#else
        Assert.Equal(original.Selector, deserialized.Selector);
#endif

        Assert.Equal(original.Type, deserialized.Type);
        Assert.Equal(original.Value, deserialized.Value);
        Assert.Equal(original.Valid, deserialized.Valid);
    }
}
