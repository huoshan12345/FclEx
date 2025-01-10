namespace FclEx.Abp.Aop;

public class LoginAndRetryAttributeTests(ITestOutputHelper output) : AbpAopTests<AbpTestModule>(output)
{
    protected override void Configure(AbpApplicationCreationOptions options, IConfigurationRoot configuration)
    {
        base.Configure(options, configuration);
        options.Services.AddUserClient<LoginAndRetryClient>();
    }

    private LoginAndRetryClient CreateClient()
    {
        var account = new UserAccount("test", "test");
        var factory = ServiceProvider.GetRequiredService<IUserClientFactory<LoginAndRetryClient>>();
        var client = factory.Create(account);
        Assert.IsNotType<LoginAndRetryClient>(client);
        Assert.IsAssignableFrom<LoginAndRetryClient>(client);
        return client;
    }

    [Fact]
    public void Aop_Test()
    {
        var factory = ServiceProvider.GetRequiredService<IUserClientFactory<LoginAndRetryClient>>();
        var client = factory.Create(new UserAccount("user", "password"));
        Assert.IsNotType<LoginAndRetryClient>(client);
        Assert.True(client.IsProxy());
    }

    [Fact]
    public async Task Login_Test()
    {
        var account = new UserAccount("user", "password");
        var factory = ServiceProvider.GetRequiredService<IUserClientFactory<LoginAndRetryClient>>();
        var client = factory.Create(account);
        var result = await client.LoginAsync();
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ReturnActionEvent_Test()
    {
        var client = CreateClient();
        Assert.False(client.IsOnline);
        var r = await client.DoAsync();
        Assert.True(r.Success);
        Assert.True(client.IsOnline);
    }

    public class LoginAndRetryClient : UserClient
    {
        public LoginAndRetryClient(ILoggerFactory loggerFactory) : base(loggerFactory: loggerFactory)
        {
        }

        [LoginAndRetry]
        public virtual Task<OperationResult> DoAsync()
        {
            return (IsOnline
                    ? Operation.Success
                    : Operation.CreateError(""))
                .ToTask();
        }

        protected override Task<OperationResult> LoginActionAsync(CancellationToken token)
        {
            return Operation.Success.ToTask();
        }
    }
}