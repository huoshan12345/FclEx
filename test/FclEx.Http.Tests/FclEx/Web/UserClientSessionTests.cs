namespace FclEx.Web;

public class UserClientSessionTests
{
    [Fact]
    public void Constructor_UsesEmptyStrings()
    {
        var session = new UserClientSession();

        Assert.Equal("", session.UserName);
    }

    [Fact]
    public void Properties_WhenAssigned_ReturnAssignedValues()
    {
        var session = new UserClientSession
        {
            UserName = "alice",
        };

        Assert.Equal("alice", session.UserName);
    }

    [Fact]
    public void UserClientIsOnline_ReturnsTrueOnlyWhenSessionStatusIsOnline()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var client = new TestUserClient(loggerFactory);

        Assert.False(client.IsOnline);

        client.State.Online();
        Assert.True(client.IsOnline);

        client.State.Offline();
        Assert.False(client.IsOnline);
    }
}
