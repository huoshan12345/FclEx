namespace FclEx.Extensions;

public class AssemblyExtensionsTests
{
    private static readonly Assembly Assembly = typeof(AssemblyExtensionsTests).Assembly;

    [Fact]
    public void OpenResource_CompleteName_ShouldReadMatchingResource()
    {
        var name = Assembly.GetManifestResourceNames()
            .Single(m => m.EndsWith("ResourceOne.SharedResource.txt", StringComparison.Ordinal));

        using var stream = Assembly.OpenResource(name);
        using var reader = new StreamReader(stream);

        Assert.Equal("resource one", reader.ReadToEnd().Trim());
    }

    [Theory]
    [InlineData("SharedResource.txt")]
    public void ResourceLookup_AmbiguousSuffix_ShouldThrow(string name)
    {
        var openException = Assert.Throws<ArgumentException>(() => Assembly.OpenResource(name));
        var getException = Assert.Throws<ArgumentException>(() => ResourceHelper.Embedded.GetStream(Assembly, name));

        Assert.Equal("name", openException.ParamName);
        Assert.Equal("name", getException.ParamName);
    }

    [Fact]
    public void IsJitOptimized_ReflectsDebuggableAttributeOptimizationSetting()
    {
        var attribute = Assembly.GetCustomAttribute<DebuggableAttribute>();

        Assert.Equal(attribute?.IsJITOptimizerDisabled != true, Assembly.IsJitOptimized());
    }
}
