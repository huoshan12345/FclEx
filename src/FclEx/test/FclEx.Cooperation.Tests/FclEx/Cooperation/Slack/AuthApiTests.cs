namespace FclEx.Cooperation.Slack;

public class AuthApiTests : IAssemblyFixture<GlobalFixture>
{
    [Fact]
    public async Task Auth_Test_Test()
    {
        var res = await SlackApi.Auth.Test();
        AssertExt.NotEmpty(res.User);
        AssertExt.NotEmpty(res.UserId);
    }
}