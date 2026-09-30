namespace FclEx.Logging.Extensions;

public class LoggerFactoryExtensionsTests
{
    [Fact]
    public void AddCollecting_ExistingLoggerWritesToReturnedProviderWithScopes()
    {
        using var factory = LoggerFactory.Create(_ => { });
        var logger = factory.CreateLogger("existing");
        var provider = factory.AddCollecting();

        using (logger.BeginScope("scope"))
        {
            logger.LogInformation("message");
        }

        var entry = Assert.Single(provider.Entries);
        Assert.Equal("existing", entry.Category);
        Assert.Equal("message", entry.Message);
        Assert.Equal(new object?[] { "scope" }, entry.Scopes);
    }

    [Fact]
    public void AddCollecting_RepeatedCalls_ReturnIndependentCollectors()
    {
        using var factory = LoggerFactory.Create(_ => { });
        var first = factory.AddCollecting();
        var second = factory.AddCollecting();
        Assert.NotSame(first, second);

        factory.CreateLogger("category").LogInformation("message");

        Assert.Equal("message", Assert.Single(first.Entries).Message);
        Assert.Equal("message", Assert.Single(second.Entries).Message);
        first.Clear();
        Assert.Empty(first.Entries);
        Assert.Single(second.Entries);
    }

    [Fact]
    public void AddCollecting_NullFactory_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            LoggerFactoryExtensions.AddCollecting(null!));
        Assert.Equal("factory", exception.ParamName);
    }

    public static readonly TheoryData<LogLevel> LogLevelCases = Enum.GetValues<LogLevel>().ToTheoryData();

    [Theory]
    [MemberData(nameof(LogLevelCases))]
    public void SetMinimumLevel_Test(LogLevel logLevel)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var fac = services.GetRequiredService<ILoggerFactory>();
        fac.SetMinimumLevel(logLevel);

        var options = (LoggerFilterOptions?)fac.GetType().InvokeMember(
            name: "_filterOptions",
            invokeAttr: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
            binder: null,
            target: fac,
            args: null);

        Assert.Equal(logLevel, options?.MinLevel);
    }
}
