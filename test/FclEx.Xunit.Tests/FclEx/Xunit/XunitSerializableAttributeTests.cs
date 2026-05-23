using static FclEx.Helpers.ReflectionHelper;

namespace FclEx.Xunit;

public partial class XunitSerializableAttributeTests
{
    public interface ITestType
    {
        int Id { get; }
        string Name { get; }
        string[] Addresses { get; }
        IEnumerable<int> Numbers { get; }
        List<string> Hobbies { get; }
    }

    [XunitSerializable]
    public partial record TestRecord(
        int Id,
        string Name,
        string[] Addresses,
        IEnumerable<int> Numbers,
        List<string> Hobbies
        ) : ITestType;


    [XunitSerializable]
    public partial record struct TestRecordStruct(
        int Id,
        string Name,
        string[] Addresses,
        IEnumerable<int> Numbers,
        List<string> Hobbies
        ) : ITestType;

    [XunitSerializable]
    public partial class ClassWithoutCtor : ITestType
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string[] Addresses { get; set; } = [];
        public IEnumerable<int> Numbers { get; set; } = [];
        public List<string> Hobbies { get; set; } = [];
    }

    [XunitSerializable]
    public partial class ClassWithCtor(
        int id,
        string name,
        string[] addresses,
        IEnumerable<int> numbers,
        List<string> hobbies
        ) : ITestType
    {
        public int Id { get; } = id;
        public string Name { get; } = name;
        public string[] Addresses { get; } = addresses;
        public IEnumerable<int> Numbers { get; } = numbers;
        public List<string> Hobbies { get; } = hobbies;
    }

    [XunitSerializable]
    public partial class ClassWithCtors(
        int id,
        string name,
        string[] addresses,
        IEnumerable<int> numbers,
        List<string> hobbies
        ) : ITestType
    {
        public int Id { get; } = id;
        public string Name { get; } = name;
        public string[] Addresses { get; } = addresses;
        public IEnumerable<int> Numbers { get; } = numbers;
        public List<string> Hobbies { get; } = hobbies;

        public ClassWithCtors(string name, string[] addresses, IEnumerable<int> numbers, List<string> hobbies)
            : this(1, name, addresses, numbers, hobbies) { }

    }

    [XunitSerializable]
    public partial record RecordWithParameterlessCtor(
        int Id,
        string Name,
        string[] Addresses,
        IEnumerable<int> Numbers,
        List<string> Hobbies
        ) : ITestType
    {
        public RecordWithParameterlessCtor() : this(1, "default", [], [], []) { }
    }

    public record Base(string Name);

    [XunitSerializable]
    public partial record Derived(
        int Id,
        string Name,
        string[] Addresses,
        IEnumerable<int> Numbers,
        List<string> Hobbies)
        : Base(Name), ITestType;

    [XunitSerializable]
    public partial record Nested(
        int Id,
        string Name,
        string[] Addresses,
        IEnumerable<int> Numbers,
        List<string> Hobbies,
        Derived Sub)
        : Base(Name), ITestType;

    [XunitSerializable]
    public partial class GenericClass<T>(
        int id,
        string name,
        string[] addresses,
        IEnumerable<int> numbers,
        List<string> hobbies,
        T value)
        : ClassWithCtor(id, name, addresses, numbers, hobbies)
    {
        public T Value { get; } = value;
    }


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

    private static void Test<T>(Func<int, string, string[], IEnumerable<int>, List<string>, T> creator, Action<T, T>? action = null) where T : ITestType, IXunitSerializable, new()
    {
        var original = creator(15, "Tom", ["addr1", "addr2"], [5, 10], ["Arts", "Music"]);
        var serializable = Assert.IsType<IXunitSerializable>(original, false);

        var info = CreateSerializationInfo();
        serializable.Serialize(info);

        Assert.Equal(original.Id, info.GetValue<int>(GetAutoBackingFieldName(nameof(original.Id))));
        Assert.Equal(original.Name, info.GetValue<string>(GetAutoBackingFieldName(nameof(original.Name))));
        Assert.Equal(original.Addresses, info.GetValue<string[]>(GetAutoBackingFieldName(nameof(original.Addresses))));

        var deserialized = new T();
        deserialized.Deserialize(info);

        Test(original, deserialized, action);
    }

    private static void Test<T>(T expected, T actual, Action<T, T>? action = null) where T : ITestType
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Addresses, actual.Addresses);
        Assert.Equal(expected.Numbers, actual.Numbers);
        Assert.Equal(expected.Hobbies, actual.Hobbies);

        action?.Invoke(expected, actual);
    }

    [Fact]
    public void Should_RoundTrip_Record_With_PrimaryCtor()
    {
        Test<TestRecord>((a, b, c, d, e) => new(a, b, c, d, e));
    }

    [Fact]
    public void Should_RoundTrip_RecordStruct_With_PrimaryCtor()
    {
        Test<TestRecordStruct>((a, b, c, d, e) => new(a, b, c, d, e));
    }

    [Fact]
    public void Should_RoundTrip_Class()
    {
        Test<ClassWithCtor>((a, b, c, d, e) => new(a, b, c, d, e));
    }

    [Fact]
    public void Should_RoundTrip_Class_Without_Ctor()
    {
        Test<ClassWithoutCtor>((a, b, c, d, e) => new()
        {
            Id = a,
            Name = b,
            Addresses = c,
            Numbers = d,
            Hobbies = e,
        });
    }

    [Fact]
    public void Should_RoundTrip_Class_With_MultipleCtors()
    {
        Test<ClassWithCtors>((a, b, c, d, e) => new(a, b, c, d, e));
    }

    [Fact]
    public void Should_Not_Conflict_With_Existing_ParameterlessCtor()
    {
        Test<RecordWithParameterlessCtor>((a, b, c, d, e) => new(a, b, c, d, e));
    }

    [Fact]
    public void Should_RoundTrip_With_BaseFields()
    {
        Test<Derived>((a, b, c, d, e) => new(a, b, c, d, e));
    }

    [Fact]
    public void Should_RoundTrip_Nested()
    {
        Test<Nested>((a, b, c, d, e) => new(a, b, c, d, e, new(a + 1, b + b, c + c, d + d, e + e)), (expected, actual) =>
        {
            Test(expected.Sub, actual.Sub);
        });
    }

    [Fact]
    public void Should_RoundTrip_GenericClass()
    {
        const string value = "generic value";
        Test<GenericClass<string>>((a, b, c, d, e) => new(a, b, c, d, e, value), (expected, actual) =>
        {
            Assert.Equal(value, expected.Value);
            Assert.Equal(value, actual.Value);
        });
    }
}
