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
}
