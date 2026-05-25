namespace FclEx.YamlDotNet;

public class YamlHelperTests
{
    [Fact]
    public void DefaultOptions_ShouldPreserveMemberNamesAndDisableAttributeScanning()
    {
        Assert.Equal(YamlNamingConvention.None, YamlSerializeOptions.Default.NamingConvention);
        Assert.Equal(YamlNamingConvention.None, YamlDeserializeOptions.Default.NamingConvention);
        Assert.False(YamlSerializeOptions.Default.UseTypeConverterAttributes);
        Assert.False(YamlDeserializeOptions.Default.UseTypeConverterAttributes);
    }

    [Fact]
    public void GetSerializer_DefaultOptions_ReturnsDefaultSerializer()
    {
        var defaultOptions = YamlSerializeOptions.Default;

        var serializer = YamlHelper.GetSerializer();

        Assert.NotNull(serializer);
        var serializer2 = YamlHelper.GetSerializer(defaultOptions);
        Assert.Same(serializer, serializer2);
    }

    [Fact]
    public void GetSerializer_CustomOptions_ReturnsCustomSerializer()
    {
        var customOptions = new YamlSerializeOptions
        {
            NamingConvention = YamlNamingConvention.SnakeCase,
        };

        var serializer = YamlHelper.GetSerializer(customOptions);

        Assert.NotNull(serializer);
        var serializer2 = YamlHelper.GetSerializer(customOptions);
        Assert.Same(serializer, serializer2);
    }

    [Fact]
    public void GetSerializer_DefaultOptions_DoNotApplyAttributedConverters()
    {
        var serializer = YamlHelper.GetSerializer();

        var yaml = serializer.Serialize(new SerializationBuilderExtensionsTests.AttributedValue("value"));

        Assert.Contains("Value:", yaml);
        Assert.DoesNotContain("converted:value", yaml);
    }

    [Fact]
    public void GetSerializer_WhenTypeConverterAttributesAreEnabledAppliesConvertersFromSpecifiedAssemblies()
    {
        var options = new YamlSerializeOptions
        {
            UseTypeConverterAttributes = true,
            TypeConverterAssemblies = [typeof(SerializationBuilderExtensionsTests.AttributedValue).Assembly],
        };

        var serializer = YamlHelper.GetSerializer(options);

        var yaml = serializer.Serialize(new SerializationBuilderExtensionsTests.AttributedValue("value"));

        Assert.Equal("converted:value", yaml.Trim());
    }

    [Theory]
    [InlineData(YamlNamingConvention.None, "TestValue:")]
    [InlineData(YamlNamingConvention.CamelCase, "testValue:")]
    [InlineData(YamlNamingConvention.SnakeCase, "test_value:")]
    public void GetSerializer_AppliesNamingConvention(YamlNamingConvention namingConvention, string expectedKey)
    {
        var serializer = YamlHelper.GetSerializer(new YamlSerializeOptions
        {
            NamingConvention = namingConvention,
        });

        var yaml = serializer.Serialize(new NamingConventionSample("value"));

        Assert.Contains(expectedKey, yaml);
    }

    [Fact]
    public void GetSerializer_WhenIndentedSequencesIsFalseUsesCompactSequenceIndentation()
    {
        var serializer = YamlHelper.GetSerializer(new YamlSerializeOptions
        {
            IndentedSequences = false,
        });

        var yaml = serializer.Serialize(new SequenceSample(["a", "b"]));

        Assert.Contains("Items:\n- a\n- b", yaml.Replace("\r\n", "\n"));
    }

    [Fact]
    public void GetDeserializer_DefaultOptions_ReturnsDefaultDeserializer()
    {
        var defaultOptions = YamlDeserializeOptions.Default;

        var deserializer = YamlHelper.GetDeserializer();

        Assert.NotNull(deserializer);
        var deserializer2 = YamlHelper.GetDeserializer(defaultOptions);
        Assert.Same(deserializer, deserializer2);
    }

    [Fact]
    public void GetDeserializer_CustomOptions_IgnoreUnmatchedProperties_ReturnsCustomDeserializer()
    {
        var customOptions = new YamlDeserializeOptions
        {
            NamingConvention = YamlNamingConvention.CamelCase,
            IgnoreUnmatchedProperties = true,
        };

        var deserializer = YamlHelper.GetDeserializer(customOptions);

        Assert.NotNull(deserializer);
        var deserializer2 = YamlHelper.GetDeserializer(customOptions);
        Assert.Same(deserializer, deserializer2);
    }

    [Fact]
    public void GetDeserializer_CustomOptions_ThrowOnUnmatchedProperties_ReturnsCustomDeserializer()
    {
        var customOptions = new YamlDeserializeOptions
        {
            NamingConvention = YamlNamingConvention.CamelCase,
            IgnoreUnmatchedProperties = false,
        };

        var deserializer = YamlHelper.GetDeserializer(customOptions);

        Assert.NotNull(deserializer);
        var deserializer2 = YamlHelper.GetDeserializer(customOptions);
        Assert.Same(deserializer, deserializer2);
    }

    [Fact]
    public void GetDeserializer_DefaultOptions_IgnoreUnmatchedProperties()
    {
        var deserializer = YamlHelper.GetDeserializer();

        var value = deserializer.Deserialize<NamingConventionSample>("TestValue: value\r\nExtra: ignored");

        Assert.Equal("value", value.TestValue);
    }

    [Fact]
    public void GetDeserializer_WhenIgnoreUnmatchedPropertiesIsFalseThrowsForExtraProperties()
    {
        var deserializer = YamlHelper.GetDeserializer(new YamlDeserializeOptions
        {
            IgnoreUnmatchedProperties = false,
        });

        Assert.Throws<YamlException>(() => deserializer.Deserialize<NamingConventionSample>("TestValue: value\r\nExtra: rejected"));
    }

    [Fact]
    public void GetDeserializer_WhenTypeConverterAttributesAreEnabledAppliesConvertersFromSpecifiedAssemblies()
    {
        var options = new YamlDeserializeOptions
        {
            UseTypeConverterAttributes = true,
            TypeConverterAssemblies = [typeof(SerializationBuilderExtensionsTests.AttributedValue).Assembly],
        };

        var deserializer = YamlHelper.GetDeserializer(options);

        var value = deserializer.Deserialize<SerializationBuilderExtensionsTests.AttributedValue>("converted:value");

        Assert.Equal("value", value.Value);
    }

    [Fact]
    public void GetDeserializer_AppliesNamingConvention()
    {
        var deserializer = YamlHelper.GetDeserializer(new YamlDeserializeOptions
        {
            NamingConvention = YamlNamingConvention.SnakeCase,
        });

        var value = deserializer.Deserialize<NamingConventionSample>("test_value: value");

        Assert.Equal("value", value.TestValue);
    }

    [Fact]
    public void GetSerializer_DifferentOptions_ReturnDifferentInstances()
    {
        var options1 = new YamlSerializeOptions
        {
            NamingConvention = YamlNamingConvention.CamelCase,
        };

        var options2 = new YamlSerializeOptions
        {
            NamingConvention = YamlNamingConvention.SnakeCase,
        };

        var serializer1 = YamlHelper.GetSerializer(options1);
        var serializer2 = YamlHelper.GetSerializer(options2);

        Assert.NotSame(serializer1, serializer2);
    }

    [Fact]
    public void GetDeserializer_DifferentOptions_ReturnDifferentInstances()
    {
        var options1 = new YamlDeserializeOptions
        {
            NamingConvention = YamlNamingConvention.CamelCase,
            IgnoreUnmatchedProperties = true,
        };

        var options2 = new YamlDeserializeOptions
        {
            NamingConvention = YamlNamingConvention.SnakeCase,
            IgnoreUnmatchedProperties = false,
        };

        var deserializer1 = YamlHelper.GetDeserializer(options1);
        var deserializer2 = YamlHelper.GetDeserializer(options2);

        Assert.NotSame(deserializer1, deserializer2);
    }

    public sealed class NamingConventionSample
    {
        public NamingConventionSample()
        {
            TestValue = "";
        }

        public NamingConventionSample(string testValue)
        {
            TestValue = testValue;
        }

        public string TestValue { get; set; }
    }

    public sealed record SequenceSample(IReadOnlyList<string> Items);
}
