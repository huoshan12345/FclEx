namespace FclEx.Xunit;

partial class XunitSerializableAttributeTests
{
    public interface IJsonTestModel<T>
    {
        Expression<Func<T, object>>? Selector { get; }
        Type? Type { get; }
        object? Value { get; }
        bool Valid { get; }
    }

    [XunitSerializable]
    public partial record TestModelJsonIgnoreOnField<T>(
        [field: JsonIgnore]
        Expression<Func<T, object>>? Selector,
        Type? Type,
        object? Value,
        bool Valid = false)
        : IJsonTestModel<T>;

    [XunitSerializable]
    public partial record TestModelJsonConverterOnField<T>(
        [field: JsonConverter(typeof(IgnoreJsonConverter))]
        Expression<Func<T, object>>? Selector,
        Type? Type,
        object? Value,
        bool Valid = false)
        : IJsonTestModel<T>;

    [XunitSerializable]
    public partial record TestModelJsonIgnoreOnProperty<T>(
        [property: JsonIgnore]
        Expression<Func<T, object>>? Selector,
        Type? Type,
        object? Value,
        bool Valid = false)
        : IJsonTestModel<T>;

    [XunitSerializable]
    public partial record TestModelJsonConverterOnProperty<T>(
        [property: JsonConverter(typeof(IgnoreJsonConverter))]
        Expression<Func<T, object>>? Selector,
        Type? Type,
        object? Value,
        bool Valid = false)
        : IJsonTestModel<T>;

    private static void AssertTestModel<TModel, T>(Func<TModel> creator, bool checkSelector = false) where TModel : IJsonTestModel<T>, IXunitSerializable
    {
        var original = creator();
        var info = CreateSerializationInfo();
        original.Serialize(info);

        var deserialized = ObjectHelper.GetUninitializedObject<TModel>();
        deserialized.Deserialize(info);

#if FCLEX_XUNIT_V3
        if (checkSelector)
        {
            Assert.Equal(original.Selector?.ToString(), deserialized.Selector?.ToString());
        }
        else
        {
            Assert.Null(deserialized.Selector);
        }
#else
        Assert.Equal(original.Selector, deserialized.Selector);
#endif

        Assert.Equal(original.Type, deserialized.Type);
        Assert.Equal(original.Value, deserialized.Value);
        Assert.Equal(original.Valid, deserialized.Valid);
    }

    [Fact]
    public void Should_RoundTrip_With_JsonIgnoreOnField()
    {
        AssertTestModel<TestModelJsonIgnoreOnField<string>, string>(() => new(m => m.Length, typeof(string), 1));
        AssertTestModel<TestModelJsonIgnoreOnField<int>, int>(() => new(m => m, typeof(string), 1));
    }

    [Fact]
    public void Should_RoundTrip_With_JsonConverterOnField()
    {
        AssertTestModel<TestModelJsonConverterOnField<string>, string>(() => new(m => m.Length, typeof(string), 1));
        AssertTestModel<TestModelJsonConverterOnField<int>, int>(() => new(m => m, typeof(string), 1));
    }

    [Fact]
    public void Should_RoundTrip_With_JsonIgnoreOnProperty()
    {
        AssertTestModel<TestModelJsonIgnoreOnProperty<string>, string>(() => new(m => m.Length, typeof(string), 1));
        AssertTestModel<TestModelJsonIgnoreOnProperty<int>, int>(() => new(m => m, typeof(string), 1));
    }

    [Fact]
    public void Should_RoundTrip_With_JsonConverterOnProperty()
    {
        AssertTestModel<TestModelJsonConverterOnProperty<string>, string>(() => new(m => m.Length, typeof(string), 1));
        AssertTestModel<TestModelJsonConverterOnProperty<int>, int>(() => new(m => m, typeof(string), 1));
    }
}
