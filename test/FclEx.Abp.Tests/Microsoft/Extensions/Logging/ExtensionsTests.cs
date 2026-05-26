namespace Microsoft.Extensions.Logging;

public class ExtensionsTests
{
    [Fact]
    public void IsNullOrNullLogger_Test()
    {
        Assert.True(NullLogger.Instance.IsNullOrNullLogger());

        Assert.True(NullLogger<int>.Instance.IsNullOrNullLogger());

        Assert.True(((ILogger?)null).IsNullOrNullLogger());

        Assert.True(((ILogger<int>?)null).IsNullOrNullLogger());

        var fac = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider()
            .GetRequiredService<ILoggerFactory>();

        Assert.False(fac.CreateLogger("test").IsNullOrNullLogger());

        Assert.False(fac.CreateLogger<ExtensionsTests>().IsNullOrNullLogger());
    }
}