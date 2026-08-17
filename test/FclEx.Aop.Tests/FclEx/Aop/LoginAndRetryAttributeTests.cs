namespace FclEx.Aop;

public class LoginAndRetryAttributeTests : AopTests
{
    private static LoginAndRetryClient CreateClient()
    {
        var account = new UserAccount("test", "test");
        var factory = Services.GetRequiredService<IUserClientFactory<LoginAndRetryClient>>();
        var client = factory.Create(account);
        Assert.True(client.IsProxy());
        Assert.IsType<LoginAndRetryClient>(client, false);
        return client;
    }

    [Fact]
    public void Aop_Test()
    {
        var factory = Services.GetRequiredService<IUserClientFactory<LoginAndRetryClient>>();
        var client = factory.Create(new UserAccount("user", "password"));
        Assert.IsNotType<LoginAndRetryClient>(client);
        Assert.True(client.IsProxy());
    }

    [Fact]
    public async Task Login_Test()
    {
        var account = new UserAccount("user", "password");
        var factory = Services.GetRequiredService<IUserClientFactory<LoginAndRetryClient>>();
        var client = factory.Create(account);
        var result = await client.LoginAsync(CancellationToken);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ReturnActionEvent_Test()
    {
        var client = CreateClient();
        Assert.False(client.IsOnline);
        var r = await client.DoAsync();
        Assert.True(r.IsSuccess);
        Assert.True(client.IsOnline);
    }
}