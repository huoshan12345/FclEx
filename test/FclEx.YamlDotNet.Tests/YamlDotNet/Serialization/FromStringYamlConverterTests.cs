#if NET6_0_OR_GREATER
namespace YamlDotNet.Serialization;

public class FromStringYamlConverterTests
{
    [Fact]
    public void Accepts_ReturnsTrueForTargetType()
    {
        var converter = new FromStringYamlConverter<TestFromStringValue>();

        var result = converter.Accepts(typeof(TestFromStringValue));

        Assert.True(result);
    }

    [Fact]
    public void Deserialize_ReturnsNullWhenScalarIsEmpty()
    {
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new FromStringYamlConverter<TestFromStringValue>())
            .Build();

        var value = deserializer.Deserialize<TestFromStringValue?>("");

        Assert.Null(value);
    }

    [Fact]
    public void Deserialize_UsesFromString()
    {
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new FromStringYamlConverter<TestFromStringValue>())
            .Build();

        var value = deserializer.Deserialize<TestFromStringValue>("abc");

        Assert.NotNull(value);
        Assert.Equal("from:abc", value.Value);
    }

    [Fact]
    public void Serialize_UsesToString()
    {
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new FromStringYamlConverter<TestFromStringValue>())
            .Build();

        var yaml = serializer.Serialize(new TestFromStringValue("abc"));

        Assert.Equal("to:abc", yaml.Trim());
    }

    [Fact]
    public void Serialize_EmitsNullForNullValue()
    {
        var converter = new FromStringYamlConverter<TestFromStringValue>();
        var emitter = new RecordingEmitter();

        converter.WriteYaml(emitter, null, null!);

        var scalar = Assert.IsType<Scalar>(Assert.Single(emitter.Events));
        Assert.Null(scalar.Value);
    }

    [Fact]
    public void Serialize_EmitsNullWhenToStringReturnsNull()
    {
        var converter = new FromStringYamlConverter<NullStringValue>();
        var emitter = new RecordingEmitter();

        converter.WriteYaml(emitter, new NullStringValue(), null!);

        var scalar = Assert.IsType<Scalar>(Assert.Single(emitter.Events));
        Assert.Null(scalar.Value);
    }

    public sealed record TestFromStringValue(string Value) : IFromString<TestFromStringValue>
    {
        public static TestFromStringValue? FromString(string? str)
        {
            return str is null ? null : new TestFromStringValue($"from:{str}");
        }

        public override string ToString()
        {
            return $"to:{Value}";
        }
    }

    public sealed class NullStringValue : IFromString<NullStringValue>
    {
        public static NullStringValue? FromString(string? str)
        {
            return str is null ? null : new NullStringValue();
        }

        public override string? ToString()
        {
            return null;
        }
    }

    private sealed class RecordingEmitter : IEmitter
    {
        public List<ParsingEvent> Events { get; } = [];

        public void Emit(ParsingEvent @event)
        {
            Events.Add(@event);
        }
    }
}
#endif
