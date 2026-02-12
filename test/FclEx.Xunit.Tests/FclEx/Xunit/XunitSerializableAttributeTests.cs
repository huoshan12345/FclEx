namespace FclEx.Xunit;

public partial class XunitSerializableAttributeTests
{
    public interface ITestType
    {
        int Id { get; }
        string Name { get; }
        string[] Addresses { get; }
    }

    [XunitSerializable]
    public partial record TestRecord(int Id, string Name, string[] Addresses) : ITestType;

    [XunitSerializable]
    public partial class ClassWithoutCtor : ITestType
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string[] Addresses { get; set; } = [];
        public IEnumerable<int> Numbers { get; set; } = [];
    }

    [XunitSerializable]
    public partial class ClassWithCtor : ITestType
    {
        public int Id { get; }
        public string Name { get; }
        public string[] Addresses { get; }
        public IEnumerable<int> Numbers { get; }

        public ClassWithCtor(int id, string name, string[] addresses, IEnumerable<int> numbers)
        {
            Id = id;
            Name = name;
            Addresses = addresses;
            Numbers = numbers;
        }
    }

    [XunitSerializable]
    public partial class ClassWithCtors : ITestType
    {
        public int Id { get; }
        public string Name { get; }
        public string[] Addresses { get; }
        public IEnumerable<int> Numbers { get; }

        public ClassWithCtors(int id, string name, string[] addresses, IEnumerable<int> numbers)
        {
            Id = id;
            Name = name;
            Addresses = addresses;
            Numbers = numbers;
        }

        public ClassWithCtors(string name, string[] addresses, IEnumerable<int> numbers) : this(1, name, addresses, numbers) { }

        public ClassWithCtors(string[] addresses, IEnumerable<int> numbers) : this("default", addresses, numbers) { }

        public ClassWithCtors(IEnumerable<int> numbers) : this(["test-addr1", "test-addr2"], numbers) { }
    }

    [XunitSerializable]
    public partial record RecordWithParameterlessCtor(int Id, string Name, string[] Addresses, IEnumerable<int> Numbers) : ITestType
    {
        public RecordWithParameterlessCtor() : this(1, "default", [], []) { }
    }

    public record Base(string Name);

    [XunitSerializable]
    public partial record Derived(int Id, string Name, string[] Addresses, IEnumerable<int> Numbers) : Base(Name), ITestType;

    [XunitSerializable]
    public partial record Nested(int Id, string Name, string[] Addresses, IEnumerable<int> Numbers, Derived Sub) : Base(Name), ITestType;


#if !FCLEX_XUNIT_V3
    private static readonly Type _serializationHelper = typeof(SerializationHelper).Assembly.GetRequiredType("Xunit.Serialization.XunitSerializationInfo");
    private static readonly ConstructorInfo _serializationHelperConstructor = _serializationHelper.GetRequiredConstructor(typeof(IXunitSerializable));
#endif

    private static IXunitSerializationInfo CreateSerializationInfo()
    {
#if FCLEX_XUNIT_V3
        return new XunitSerializationInfo(SerializationHelper.Instance);
#else
        return _serializationHelperConstructor.Invoke<IXunitSerializationInfo>([null]);
#endif
    }

    private static string GetAutoFieldName(string propertyName) => $"<{propertyName}>k__BackingField";

    private static void Test<T>(Func<int, string, string[], IEnumerable<int>, T> creator, Action<T, T>? action = null) where T : class, ITestType, new()
    {
        var original = creator(15, "Tom", ["addr1", "addr2"], [5, 10]);
        var serializable = Assert.IsType<IXunitSerializable>(original, false);

        var info = CreateSerializationInfo();
        serializable.Serialize(info);

        Assert.Equal(original.Id, info.GetValue<int>(GetAutoFieldName(nameof(original.Id))));
        Assert.Equal(original.Name, info.GetValue<string>(GetAutoFieldName(nameof(original.Name))));
        Assert.Equal(original.Addresses, info.GetValue<string[]>(GetAutoFieldName(nameof(original.Addresses))));
        Assert.Equal(original.Numbers, info.GetValue<IEnumerable<int>>(GetAutoFieldName(nameof(original.Numbers))));

        var deserialized = new T();
        ((IXunitSerializable)deserialized).Deserialize(info);

        Test(original, deserialized, action);
    }

    private static void Test<T>(T expected, T actual, Action<T, T>? action = null) where T : class, ITestType
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Addresses, actual.Addresses);
        Assert.Equal(expected.Numbers, actual.Numbers);

        action?.Invoke(expected, actual);
    }

    [Fact]
    public void Should_RoundTrip_Record_With_PrimaryCtor()
    {
        Test<TestRecord>((a, b, c, d) => new(a, b, c, d));
    }

    [Fact]
    public void Should_RoundTrip_Class()
    {
        Test<ClassWithCtor>((a, b, c, d) => new(a, b, c, d));
    }

    [Fact]
    public void Should_RoundTrip_Class_Without_Ctor()
    {
        Test<ClassWithoutCtor>((a, b, c, d) => new() { Id = a, Name = b, Addresses = c, Numbers = d });
    }

    [Fact]
    public void Should_RoundTrip_Class_With_MultipleCtors()
    {
        Test<ClassWithCtors>((a, b, c, d) => new(a, b, c, d));
    }

    [Fact]
    public void Should_Not_Conflict_With_Existing_ParameterlessCtor()
    {
        Test<RecordWithParameterlessCtor>((a, b, c, d) => new(a, b, c, d));
    }

    [Fact]
    public void Should_RoundTrip_With_BaseFields()
    {
        Test<Derived>((a, b, c, d) => new(a, b, c, d));
    }

    [Fact]
    public void Should_RoundTrip_Nested()
    {
        Test<Nested>((a, b, c, d) => new(a, b, c, d, new(a + 1, b + b, c + c, d + d)), (expected, actual) =>
        {
            Test(expected.Sub, actual.Sub);
        });
    }
}
