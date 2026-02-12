namespace FclEx.Xunit;

public partial class XunitSerializableAttributeTests
{
    public interface ITestType
    {
        int Id { get; }
        string Name { get; }
    }

    [XunitSerializable]
    public partial record TestRecord(int Id, string Name) : ITestType;

    [XunitSerializable]
    public partial class ClassWithoutCtor : ITestType
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    [XunitSerializable]
    public partial class ClassWithCtor : ITestType
    {
        public int Id { get; }
        public string Name { get; }

        public ClassWithCtor(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    [XunitSerializable]
    public partial class ClassWithCtors : ITestType
    {
        public int Id { get; }
        public string Name { get; }

        public ClassWithCtors(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public ClassWithCtors(string name) : this(1, name) { }

        public ClassWithCtors() : this(1, "default") { }
    }

    [XunitSerializable]
    public partial record RecordWithParameterlessCtor(int Id, string Name) : ITestType
    {
        public RecordWithParameterlessCtor() : this(1, "default") { }
    }

    public record Base(string Name);

    [XunitSerializable]
    public partial record Derived(int Id, string Name) : Base(Name), ITestType;


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

    private static void Test<T>(Func<int, string, T> creator) where T : class, ITestType, new()
    {
        var original = creator(5, "/usr/docs");
        var serializable = Assert.IsType<IXunitSerializable>(original, false);

        var info = CreateSerializationInfo();
        serializable.Serialize(info);

        Assert.Equal(original.Id, info.GetValue<int>(GetAutoFieldName(nameof(original.Id))));
        Assert.Equal(original.Name, info.GetValue<string>(GetAutoFieldName(nameof(original.Name))));

        var deserialized = new T();
        ((IXunitSerializable)deserialized).Deserialize(info);

        Assert.NotSame(original, deserialized);
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Name, deserialized.Name);
    }

    [Fact]
    public void Should_RoundTrip_Record_With_PrimaryCtor()
    {
        Test<TestRecord>((i, s) => new(i, s));
    }

    [Fact]
    public void Should_RoundTrip_Class()
    {
        Test<ClassWithCtor>((i, s) => new(i, s));
    }

    [Fact]
    public void Should_RoundTrip_Class_Without_Ctor()
    {
        Test<ClassWithoutCtor>((i, s) => new() { Id = i, Name = s });
    }

    [Fact]
    public void Should_RoundTrip_Class_With_MultipleCtors()
    {
        Test<ClassWithCtors>((i, s) => new(i, s));
    }

    [Fact]
    public void Should_Not_Conflict_With_Existing_ParameterlessCtor()
    {
        Test<RecordWithParameterlessCtor>((i, s) => new(i, s));
    }

    [Fact]
    public void Should_RoundTrip_With_BaseFields()
    {
        Test<Derived>((i, s) => new(i, s));
    }
}
