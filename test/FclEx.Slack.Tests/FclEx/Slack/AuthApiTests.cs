namespace FclEx.Slack;

public class AuthApiTests : SlackTests
{
    [RetryFact]
    public async Task Auth_Test_Test()
    {
        var res = await SlackApi.Auth.Test();
        Assert.NotNullNorEmpty(res.User);
        Assert.NotNullNorEmpty(res.UserId);
    }
}