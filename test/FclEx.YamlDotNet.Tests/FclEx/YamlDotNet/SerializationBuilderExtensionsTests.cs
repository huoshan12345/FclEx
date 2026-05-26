namespace FclEx.YamlDotNet;

public class SerializationBuilderExtensionsTests
{
    [Fact]
    public void SerializerBuilder_WithAttributedTypeConverters_RegistersConvertersFromAssembly()
    {
        var serializer = new SerializerBuilder()
            .WithAttributedTypeConverters(typeof(AttributedValue).Assembly)
            .Build();

        var yaml = serializer.Serialize(new AttributedValue("value"));

        Assert.Equal("converted:value", yaml.Trim());
    }

    [Fact]
    public void SerializerBuilder_WithAttributedTypeConverters_RegistersConvertersFromAssemblies()
    {
        var serializer = new SerializerBuilder()
            .WithAttributedTypeConverters([typeof(AttributedValue).Assembly])
            .Build();

        var yaml = serializer.Serialize(new AttributedValue("value"));

        Assert.Equal("converted:value", yaml.Trim());
    }

    [Fact]
    public void SerializerBuilder_WithAttributedTypeConvertersFromCurrentAppDomain_RegistersConverters()
    {
        var serializer = new SerializerBuilder()
            .WithAttributedTypeConvertersFromCurrentAppDomain()
            .Build();

        var yaml = serializer.Serialize(new AttributedValue("value"));

        Assert.Equal("converted:value", yaml.Trim());
    }

    [Fact]
    public void DeserializerBuilder_WithAttributedTypeConverters_RegistersConvertersFromAssembly()
    {
        var deserializer = new DeserializerBuilder()
            .WithAttributedTypeConverters(typeof(AttributedValue).Assembly)
            .Build();

        var value = deserializer.Deserialize<AttributedValue>("converted:value");

        Assert.Equal("value", value.Value);
    }

    [Fact]
    public void DeserializerBuilder_WithAttributedTypeConverters_RegistersConvertersFromAssemblies()
    {
        var deserializer = new DeserializerBuilder()
            .WithAttributedTypeConverters([typeof(AttributedValue).Assembly])
            .Build();

        var value = deserializer.Deserialize<AttributedValue>("converted:value");

        Assert.Equal("value", value.Value);
    }

    [Fact]
    public void DeserializerBuilder_WithAttributedTypeConvertersFromCurrentAppDomain_RegistersConverters()
    {
        var deserializer = new DeserializerBuilder()
            .WithAttributedTypeConvertersFromCurrentAppDomain()
            .Build();

        var value = deserializer.Deserialize<AttributedValue>("converted:value");

        Assert.Equal("value", value.Value);
    }

    [Fact]
    public void WithAttributedTypeConverters_IgnoresAssembliesWithoutAttributedTypes()
    {
        var exception = Record.Exception(() => new SerializerBuilder().WithAttributedTypeConverters(typeof(string).Assembly));

        Assert.Null(exception);
    }

    [Fact]
    public void WithAttributedTypeConverters_IgnoresAbstractAttributedTypes()
    {
        var serializer = new SerializerBuilder()
            .WithAttributedTypeConverters(typeof(AbstractAttributedValue).Assembly)
            .Build();

        var yaml = serializer.Serialize(new AttributedValue("value"));

        Assert.Equal("converted:value", yaml.Trim());
    }

    [YamlTypeConverter(typeof(AttributedValueYamlConverter))]
    public sealed record AttributedValue(string Value);

    [YamlTypeConverter(typeof(AttributedValueYamlConverter))]
    public abstract record AbstractAttributedValue(string Value);

    public sealed class AttributedValueYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(AttributedValue);
        }

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            var value = parser.Consume<Scalar>().Value;
            return new AttributedValue(value["converted:".Length..]);
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            var attributedValue = Assert.IsType<AttributedValue>(value);
            emitter.Emit($"converted:{attributedValue.Value}");
        }
    }

}
