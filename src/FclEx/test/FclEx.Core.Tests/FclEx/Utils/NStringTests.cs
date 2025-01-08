namespace FclEx.Utils;

public class NStringTests
{
    [Fact]
    public void Equals_Test()
    {
        Assert.Equal(default, new NString(""));
        Assert.Equal(new NString(""), new NString(""));
        Assert.Equal(new NString(""), new NString(null));
        Assert.Equal(new NString("test"), new NString("test"));
    }

    [Fact]
    public void NotEquals_Test()
    {
        Assert.False(new NString("test").Equals(null));
        Assert.NotEqual(new NString(""), new NString("test"));
        Assert.NotEqual(new NString(null), new NString("test"));
        Assert.NotEqual(new NString("test"), new NString("test2"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("test")]
    public void ToJson_Test(string? str)
    {
        NString nstr = str;
        Assert.Equal(nstr.Value.ToJson(), nstr.ToJson());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("test")]
    public void FromJson_Test(string? str)
    {
        var json = str.ToJson();
        var token = json.ToJsonNode();
        Assert.Equal(token.Deserialize<string>() ?? "", token.Deserialize<NString>());
    }
}