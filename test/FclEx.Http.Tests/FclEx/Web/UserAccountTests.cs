namespace FclEx.Web;

public class UserAccountTests
{
    [Fact]
    public void ToString_ReturnsUserName()
    {
        var account = new UserAccount("alice", "password");

        Assert.Equal("alice", account.ToString());
    }

    [Fact]
    public void Empty_UsesEmptyUserNameAndPassword()
    {
        Assert.Equal("", UserAccount.Empty.UserName);
        Assert.Equal("", UserAccount.Empty.Password);
        Assert.Equal("", UserAccount.Empty.ToString());
    }
}
