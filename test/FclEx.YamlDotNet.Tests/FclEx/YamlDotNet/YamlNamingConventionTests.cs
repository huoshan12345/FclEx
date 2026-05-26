namespace FclEx.YamlDotNet;

public class YamlNamingConventionTests
{
    [Theory]
    [InlineData(YamlNamingConvention.None, "TestValue", "TestValue")]
    [InlineData(YamlNamingConvention.CamelCase, "TestValue", "testValue")]
    [InlineData(YamlNamingConvention.KebabCase, "TestValue", "test-value")]
    [InlineData(YamlNamingConvention.LowerCase, "TestValue", "testvalue")]
    [InlineData(YamlNamingConvention.SnakeCase, "TestValue", "test_value")]
    [InlineData(YamlNamingConvention.PascalCase, "test_value", "TestValue")]
    public void ToNamingConvention_ConvertsNames(YamlNamingConvention convention, string value, string expected)
    {
        var namingConvention = convention.ToNamingConvention();

        var converted = namingConvention.Apply(value);

        Assert.Equal(expected, converted);
    }

    [Fact]
    public void ToNamingConvention_ThrowsWhenConventionIsUnknown()
    {
        var convention = (YamlNamingConvention)999;

        Assert.Throws<ArgumentOutOfRangeException>(() => convention.ToNamingConvention());
    }
}
