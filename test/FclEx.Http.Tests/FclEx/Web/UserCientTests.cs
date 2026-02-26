namespace FclEx.Web;

public class UserCientTests : WebTests
{
    public UserCientTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Log_Test()
    {
        var account = new UserAccount("user", "password");
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>());
        client.Logger.LogInformation("test");
        client.Account = account;
        client.Logger.LogInformation("test");
    }

    [Fact]
    public async Task Login_Test()
    {
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>());
        client.Account = new UserAccount("user", "password");
        var result = await client.LoginAsync();
        Assert.True(result.IsSuccess);
    }
}