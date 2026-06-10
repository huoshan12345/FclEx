namespace FclEx.Http.Helpers;

public class CookieHelperTests
{
    [Fact]
    public void Parse_WhenCookieStringIsEmpty_ReturnsNoResults()
    {
        var results = CookieHelper.Parse("");

        Assert.Empty(results);
    }

    [Fact]
    public void Parse_WhenCookieHasEmptyValue_PreservesEmptyValueAndAttributes()
    {
        var result = Assert.Single(CookieHelper.Parse("sid=; Path=/account; Secure; HttpOnly"));

        Assert.True(result.IsSuccess);
        Assert.Equal("sid", result.Value!.Name);
        Assert.Equal("", result.Value.Value);
        Assert.Equal("/account", result.Value.Path);
        Assert.True(result.Value.Secure);
        Assert.True(result.Value.HttpOnly);
    }

    [Fact]
    public void Parse_WhenAttributeAppearsMoreThanOnce_UsesFirstAttributeValue()
    {
        var result = Assert.Single(CookieHelper.Parse("sid=abc; Path=/first; Path=/second"));

        Assert.True(result.IsSuccess);
        Assert.Equal("/first", result.Value!.Path);
    }

    [Fact]
    public void Parse_WhenExpiresUsesTwoDigitYearWithHyphen_ParsesCookie()
    {
        var result = Assert.Single(CookieHelper.Parse("sid=abc; Expires=Wed, 09-Nov-99 23:12:40 GMT"));

        Assert.True(result.IsSuccess);
        Assert.Equal("sid", result.Value!.Name);
        Assert.Equal("abc", result.Value.Value);
        Assert.Equal(1999, result.Value.Expires.ToUniversalTime().Year);
    }
}
