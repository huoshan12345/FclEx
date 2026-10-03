namespace FclEx.Logging;

public class LoggingTests
{
    [Fact]
    public void Log_NStringAsArg_Test()
    {
        var provider = new CollectingLoggerProvider();

        using var factory = LoggerFactory.Create(b =>
        {
            b.AddProvider(provider);
        });

        var logger = factory.CreateLogger("test");

        NString name = "test12345";
        logger.LogInformation("UserName: {UserName}", name);

        var entry = Assert.Single(provider.Entries);
        Assert.Equal($"UserName: {name.Value}", entry.Message);
    }
}
