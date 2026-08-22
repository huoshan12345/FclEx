namespace FclEx.Web;

public class UserClientFactoryTests : WebTests
{
    [Fact]
    public void ServiceProvider_ResolvesNonGenericAndGenericFactories()
    {
        var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient>>();
        Assert.IsType<UserClientFactory<TestUserClient>>(factory);

        var genericFactory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient, IUserAccount>>();
        Assert.IsType<UserClientFactory<TestUserClient, IUserAccount>>(genericFactory);
    }

    [Fact]
    public void FactoryCreate_CreatesClientWithAccountAndWrappedLogger()
    {
        var account = new UserAccount("test", "test");
        var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient>>();
        var client = factory.Create(account);
        Assert.NotNull(client);
        Assert.Equal(client.Account, account);
        Assert.IsType<UserClientLogger>(client.Logger);

        var innerLogger = typeof(UserClientLogger)
            .GetRequiredField("_logger", true)
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

        Assert.Equal(typeof(XunitLoggerProvider), providerType);

    }

    [Fact]
    public void ServiceProviderCreateUserClient_CreatesClientWithAccount()
    {
        var account = new UserAccount("test", "test");

        var client = ServiceProvider.CreateUserClient<TestUserClient>(account);

        Assert.Equal(account, client.Account);
        Assert.IsType<TestUserClient>(client);
    }

    [Fact]
    public void ServiceProviderCreateUserClient_WhenHttpServiceIsProvided_AssignsService()
    {
        var account = new UserAccount("test", "test");
        using var httpService = new HttpClientService();

        var client = ServiceProvider.CreateUserClient<TestUserClient>(account, httpService);

        Assert.Equal(account, client.Account);
        Assert.Same(httpService, client.HttpService);
    }

    [Fact]
    public void ServiceProviderCreateUserClient_WithGenericAccount_CreatesClientWithTypedAccount()
    {
        var account = new TestAccount("typed", "pwd");
        var provider = new ServiceCollection()
            .AddLogging()
            .AddUserClient<TestGenericUserClient, TestAccount>(new TestAccount("empty", "pwd"))
            .BuildServiceProvider();

        var client = provider.CreateUserClient<TestGenericUserClient, TestAccount>(account);

        Assert.Same(account, client.Account);
    }

    [Fact]
    public void FactoryCreate_WhenHttpServiceIsProvided_AssignsServiceAndLogger()
    {
        var account = new UserAccount("test", "test");
        var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient>>();
        using var httpService = new HttpClientService();

        var client = factory.Create(account, httpService);

        Assert.Same(httpService, client.HttpService);
        Assert.Same(client.Logger, httpService.Logger);
    }

    [Fact]
    public void AddUserClientGenericAccount_RegistersFactoryClientAndEmptyAccount()
    {
        var emptyAccount = new TestAccount("empty", "pwd");
        var provider = new ServiceCollection()
            .AddLogging()
            .AddUserClient<TestGenericUserClient, TestAccount>(emptyAccount)
            .BuildServiceProvider();

        var factory = provider.GetRequiredService<IUserClientFactory<TestGenericUserClient, TestAccount>>();
        var resolvedAccount = provider.GetRequiredService<TestAccount>();
        var client = provider.GetRequiredService<TestGenericUserClient>();

        Assert.IsType<UserClientFactory<TestGenericUserClient, TestAccount>>(factory);
        Assert.Same(emptyAccount, resolvedAccount);
        Assert.Equal(emptyAccount, client.Account);
    }

    [Fact]
    public void AddUserClient_RegistersEmptyAccountAsBothInterfaceAndConcreteType()
    {
        var provider = new ServiceCollection()
            .AddLogging()
            .AddUserClient<TestUserClient>()
            .BuildServiceProvider();

        Assert.Same(UserAccount.Empty, provider.GetRequiredService<IUserAccount>());
        Assert.Same(UserAccount.Empty, provider.GetRequiredService<UserAccount>());
    }

    [Fact]
    public void AddUserClient_DoesNotReplaceExistingRegistrations()
    {
        var account = new UserAccount("existing", "pwd");
        var provider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IUserAccount>(account)
            .AddSingleton<UserAccount>(account)
            .AddSingleton<IUserClientFactory<TestUserClient>>(new ExistingFactory())
            .AddUserClient<TestUserClient>()
            .BuildServiceProvider();

        Assert.Same(account, provider.GetRequiredService<IUserAccount>());
        Assert.Same(account, provider.GetRequiredService<UserAccount>());
        Assert.IsType<ExistingFactory>(provider.GetRequiredService<IUserClientFactory<TestUserClient>>());
    }

    [Fact]
    public void FactoryCreate_WithWebProxy_AssignsProxyToClientService()
    {
        var account = new UserAccount("test", "test");
        var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient>>();
        var client = factory.Create(account);
        Assert.Null(client.HttpService.Proxy);

        var proxy = WebProxy.Create("http://localhost:8888");
        client = factory.Create(account, proxy);
        Assert.Equal(proxy, client.HttpService.Proxy);
    }

    [Theory]
    [InlineData("http://localhost:8888")]
    [InlineData(null)]
    public void Create_WithStringProxy_CreatesHttpServiceWithProxy(string? proxy)
    {
        var account = new UserAccount("test", "test");
        var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient>>();

        var client = factory.Create(account, proxy);

        Assert.True(WebProxyInterfaceEqualityComparer.Instance.Equals(
            WebProxy.Create(proxy),
            client.HttpService.Proxy));
    }

    [Fact]
    public void Create_WithUriProxy_CreatesHttpServiceWithProxy()
    {
        var account = new UserAccount("test", "test");
        var factory = ServiceProvider.GetRequiredService<IUserClientFactory<TestUserClient>>();
        var proxy = new Uri("http://localhost:8888");

        var client = factory.Create(account, proxy);

        Assert.True(WebProxyInterfaceEqualityComparer.Instance.Equals(
            WebProxy.Create(proxy),
            client.HttpService.Proxy));
    }

    private sealed record TestAccount(string UserName, string Password) : IUserAccount
    {
        public override string ToString() => UserName;
    }

    private sealed class TestGenericUserClient(TestAccount account, ILoggerFactory loggerFactory)
        : UserClient<TestAccount>(account, loggerFactory)
    {
        protected override Task<OperationResult> LoginActionAsync(CancellationToken token)
        {
            return Operation.Success();
        }
    }

    private sealed class ExistingFactory : IUserClientFactory<TestUserClient>
    {
        public IServiceProvider ServiceProvider { get; } = new ServiceCollection().BuildServiceProvider();

        public TestUserClient Create(IUserAccount account, IHttpService? httpService = null)
        {
            throw new NotSupportedException();
        }
    }
}
