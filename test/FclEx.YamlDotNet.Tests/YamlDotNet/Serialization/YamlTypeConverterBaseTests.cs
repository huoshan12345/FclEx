namespace YamlDotNet.Serialization;

public class YamlTypeConverterBaseTests
{
    [Fact]
    public void Accepts_ReturnsTrueForExactTargetType()
    {
        var converter = new TestYamlConverter();

        var result = converter.Accepts(typeof(TestValue));

        Assert.True(result);
    }

    [Fact]
    public void Accepts_ReturnsFalseForDerivedType()
    {
        var converter = new TestYamlConverter();

        var result = converter.Accepts(typeof(DerivedTestValue));

        Assert.False(result);
    }

    [Fact]
    public void ReadYaml_NonGenericOverloadDelegatesToTypedOverload()
    {
        var converter = new TestYamlConverter();
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(converter)
            .Build();

        var value = deserializer.Deserialize<TestValue>("name");

        Assert.Equal("name", value.Value);
    }

    [Fact]
    public void WriteYaml_NonGenericOverloadDelegatesToTypedOverload()
    {
        var converter = new TestYamlConverter();
        var serializer = new SerializerBuilder()
            .WithTypeConverter(converter)
            .Build();

        var yaml = serializer.Serialize(new TestValue("name"));

        Assert.Equal("name", yaml.Trim());
    }

    private record TestValue(string Value);

    private sealed record DerivedTestValue(string Value) : TestValue(Value);

    private sealed class TestYamlConverter : YamlTypeConverterBase<TestValue>
    {
        public override TestValue? ReadYaml(IParser parser, ObjectDeserializer deserializer)
        {
            return new TestValue(parser.Consume<Scalar>().Value);
        }

        public override void WriteYaml(IEmitter emitter, TestValue? value, ObjectSerializer serializer)
        {
            emitter.Emit(value?.Value ?? null!);
        }
    }
}
