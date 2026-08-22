namespace FclEx.Http.Extensions;

public class CookieExtensionsTests
{
    [Fact]
    public void Parse_WhenCookieStringIsEmpty_ReturnsNoResults()
    {
        var results = Cookie.Parse("");

        Assert.Empty(results);
    }

    [Fact]
    public void Parse_WhenCookieHasEmptyValue_PreservesEmptyValueAndAttributes()
    {
        var result = Assert.Single(Cookie.Parse("sid=; Path=/account; Secure; HttpOnly"));

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
        var result = Assert.Single(Cookie.Parse("sid=abc; Path=/first; Path=/second"));

        Assert.True(result.IsSuccess);
        Assert.Equal("/first", result.Value!.Path);
    }

    [Fact]
    public void Parse_WhenExpiresUsesTwoDigitYearWithHyphen_ParsesCookie()
    {
        var result = Assert.Single(Cookie.Parse("sid=abc; Expires=Wed, 09-Nov-99 23:12:40 GMT"));

        Assert.True(result.IsSuccess);
        Assert.Equal("sid", result.Value!.Name);
        Assert.Equal("abc", result.Value.Value);
        Assert.Equal(1999, result.Value.Expires.ToUniversalTime().Year);
    }

    [Fact]
    public void Parse_WhenExpiresContainsComma_DoesNotTreatDateCommaAsNextCookie()
    {
        var results = Cookie.Parse("sid=abc; Expires=Wed, 09 Nov 2030 23:12:40 GMT, theme=dark").ToList();

        Assert.Equal(2, results.Count);
        Assert.True(results[0].IsSuccess, results[0].Exception?.ToString());
        Assert.True(results[1].IsSuccess, results[1].Exception?.ToString());
        Assert.Equal("sid", results[0].Value!.Name);
        Assert.Equal("theme", results[1].Value!.Name);
    }

    [Fact]
    public void Parse_WhenMaxAgeIsValid_SetsFutureExpiration()
    {
        var before = DateTime.UtcNow;

        var result = Assert.Single(Cookie.Parse("sid=abc; Max-Age=60"));

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.InRange(result.Value!.Expires, before.AddSeconds(55), DateTime.UtcNow.AddSeconds(65));
    }

    [Fact]
    public void Parse_WhenDomainIsQuoted_RemovesQuotes()
    {
        var result = Assert.Single(Cookie.Parse("sid=abc; Domain=\".example.com\""));

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(".example.com", result.Value!.Domain);
    }

    [Fact]
    public void Parse_WhenExpiresIsInvalid_ReturnsErrorForIgnoredCookie()
    {
        var result = Assert.Single(Cookie.Parse("sid=abc; Expires=not-a-date"));

        Assert.True(result.IsError);
        Assert.Contains("empty name", result.Exception!.Message);
    }
}
