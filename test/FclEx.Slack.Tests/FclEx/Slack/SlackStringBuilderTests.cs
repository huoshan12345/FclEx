namespace FclEx.Slack;

public class SlackStringBuilderTests
{
    [Fact]
    public void AppendUser_Test()
    {
        const string userId = "U123456";
        var str = SlackStringBuilder.Build(m => m.AppendUser(userId));
        Assert.Equal($"<@{userId}>", str);
    }
}
