namespace System.ComponentModel.DataAnnotations;

public class UriAttributeTests
{
    [Fact]
    public void Null_Should_Be_Valid()
    {
        var attr = new UriAttribute();
        Assert.True(attr.IsValid(null));
    }

    [Fact]
    public void Non_String_Should_Be_Invalid()
    {
        var attr = new UriAttribute();
        Assert.False(attr.IsValid(123));
    }

    [Fact]
    public void Empty_String_Default_Should_Be_Valid()
    {
        var attr = new UriAttribute();
        Assert.True(attr.IsValid(""));
    }

    [Fact]
    public void Empty_String_Not_Allowed_Should_Be_Invalid()
    {
        var attr = new UriAttribute { AllowEmptyStrings = false };
        Assert.False(attr.IsValid(""));
    }

    [Fact]
    public void Whitespace_Default_Should_Be_Valid()
    {
        var attr = new UriAttribute();
        Assert.True(attr.IsValid("   "));
    }

    [Fact]
    public void Whitespace_Not_Allowed_Should_Be_Invalid()
    {
        var attr = new UriAttribute { AllowEmptyStrings = false };
        Assert.False(attr.IsValid("   "));
    }

    [Fact]
    public void Invalid_Uri_Should_Be_Invalid()
    {
        var attr = new UriAttribute();
        Assert.False(attr.IsValid("not_a_uri"));
    }

    [Fact]
    public void Valid_Absolute_Uri_Should_Be_Valid()
    {
        var attr = new UriAttribute();
        Assert.True(attr.IsValid("https://example.com"));
    }

    [Fact]
    public void Relative_Uri_Should_Be_Invalid()
    {
        var attr = new UriAttribute();
        Assert.False(attr.IsValid("/relative/path"));
    }

    [Fact]
    public void AllowedSchemes_Empty_Should_Allow_Any_Scheme()
    {
        var attr = new UriAttribute();
        Assert.True(attr.IsValid("ftp://example.com"));
    }

    [Fact]
    public void AllowedSchemes_Should_Restrict_Scheme()
    {
        var attr = new UriAttribute
        {
            AllowedSchemes = ["https"]
        };

        Assert.True(attr.IsValid("https://example.com"));
        Assert.False(attr.IsValid("http://example.com"));
    }

    [Fact]
    public void AllowedSchemes_Should_Be_Case_Insensitive()
    {
        var attr = new UriAttribute
        {
            AllowedSchemes = ["HTTPS"]
        };

        Assert.True(attr.IsValid("https://example.com"));
    }

    [Fact]
    public void FormatErrorMessage_No_Schema()
    {
        var attr = new UriAttribute();
        var msg = attr.FormatErrorMessage("Url");

        Assert.Equal("The Url field is not a valid URI.", msg);
    }

    [Fact]
    public void FormatErrorMessage_With_Schema()
    {
        var attr = new UriAttribute
        {
            AllowedSchemes = ["http", "https"]
        };

        var msg = attr.FormatErrorMessage("Url");

        Assert.Equal("The Url field is not a valid URI with one of the allowed schemes: http, https.", msg);
    }
}
