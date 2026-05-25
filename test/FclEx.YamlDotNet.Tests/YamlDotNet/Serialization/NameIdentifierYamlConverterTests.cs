#if NET6_0_OR_GREATER
namespace YamlDotNet.Serialization;

public class NameIdentifierYamlConverterTests
{
    [Fact]
    public void Accepts_ReturnsTrueForTargetType()
    {
        var converter = new NameIdentifierYamlConverter<TestNameIdentifier>();

        var result = converter.Accepts(typeof(TestNameIdentifier));

        Assert.True(result);
    }

    [Fact]
    public void Deserialize_ReturnsNullWhenScalarIsEmpty()
    {
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new NameIdentifierYamlConverter<TestNameIdentifier>())
            .Build();

        var value = deserializer.Deserialize<TestNameIdentifier?>("");

        Assert.Null(value);
    }

    [Fact]
    public void Deserialize_UsesCreate()
    {
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new NameIdentifierYamlConverter<TestNameIdentifier>())
            .Build();

        var value = deserializer.Deserialize<TestNameIdentifier>("abc");

        Assert.NotNull(value);
        Assert.Equal("created:abc", value.Name);
    }

    [Fact]
    public void Serialize_UsesName()
    {
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new NameIdentifierYamlConverter<TestNameIdentifier>())
            .Build();

        var yaml = serializer.Serialize(new TestNameIdentifier("abc"));

        Assert.Equal("abc", yaml.Trim());
    }

    [Fact]
    public void Serialize_EmitsNullForNullValue()
    {
        var converter = new NameIdentifierYamlConverter<TestNameIdentifier>();
        var emitter = new RecordingEmitter();

        converter.WriteYaml(emitter, null, null!);

        var scalar = Assert.IsType<Scalar>(Assert.Single(emitter.Events));
        Assert.Null(scalar.Value);
    }

    public sealed record TestNameIdentifier(string Name) : INameIdentifier<TestNameIdentifier>
    {
        public static TestNameIdentifier Create(string name)
        {
            return new TestNameIdentifier($"created:{name}");
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
