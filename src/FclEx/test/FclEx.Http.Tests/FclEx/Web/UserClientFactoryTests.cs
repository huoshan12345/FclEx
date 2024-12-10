using FclEx.Logging;
using FclEx.Xunit;
using Microsoft.Extensions.Logging;

namespace FclEx.Web;

public class UserClientFactoryTests : WebTests
{
    public UserClientFactoryTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Resolve_Test()
    {
        var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient>>();
        Assert.IsType<UserClientFactory<TestUserClient>>(factory);
    }

    [Fact]
    public void Create_Test()
    {
        var account = new UserAccount("test", "test");
        var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient>>();
        var client = factory.Create(account);
        Assert.NotNull(client);
        Assert.Equal(client.Account, account);
        Assert.IsType<UserClientLogger>(client.Logger);

        var innerLogger = typeof(UserClientLogger)
            .GetRequiredField("_logger")
            .GetRequiredValue<ILogger>(client.Logger);
        Assert.IsType<PropertiesLogger>(innerLogger);

        var actualLogger = typeof(PropertiesLogger)
            .GetRequiredField("_logger")
            .GetRequiredValue<ILogger>(innerLogger);
        var loggerType = actualLogger.GetType();
        Assert.Equal("Microsoft.Extensions.Logging.Logger", loggerType.LongName());
        Assert.True(actualLogger.IsEnabled(LogLevel.Trace));

        var loggers = loggerType.GetRequiredProperty("Loggers").GetRequiredValue<Array>(actualLogger);
        Assert.Single(loggers);

        var loggerInfo = loggers.GetValue(0);
        Assert.NotNull(loggerInfo);

        var providerType = loggerInfo.GetType()
            .GetRequiredProperty("ProviderType")
            .GetRequiredValue<Type>(loggerInfo);

        Assert.Equal(typeof(TestLoggerProvider), providerType);

    }

    [Fact]
    public void Create_WithProxy_Test()
    {
        var account = new UserAccount("test", "test");
        var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient>>();
        var client = factory.Create(account);
        Assert.Null(client.HttpService.Proxy);

        var proxy = WebProxyHelper.Create("http://localhost:8888");
        client = factory.Create(account, proxy);
        Assert.Equal(proxy, client.HttpService.Proxy);
    }
}