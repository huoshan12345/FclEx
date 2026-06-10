namespace MimeTypes;

public class MimeTypeMapTests
{
    [Theory]
    [InlineData("index.HTML", "text/html")]
    [InlineData(".json", "application/json")]
    [InlineData("archive.tar.gz?download=1", "application/x-gzip")]
    public void TryGetMimeType_WhenExtensionIsKnown_ReturnsMimeType(string input, string expected)
    {
        var found = MimeTypeMap.TryGetMimeType(input, out var mimeType);

        Assert.True(found);
        Assert.Equal(expected, mimeType);
    }

    [Fact]
    public void GetMimeType_WhenExtensionIsUnknown_ReturnsDefaultMimeType()
    {
        var mimeType = MimeTypeMap.GetMimeType("file.unknown-extension");

        Assert.Equal("application/octet-stream", mimeType);
    }

    [Theory]
    [InlineData("text/html", ".html")]
    [InlineData("application/json", ".json")]
    public void GetExtension_WhenMimeTypeIsKnown_ReturnsPreferredExtension(string mimeType, string expected)
    {
        var extension = MimeTypeMap.GetExtension(mimeType);

        Assert.Equal(expected, extension);
    }

    [Fact]
    public void GetExtension_WhenMimeTypeIsUnknownAndThrowIsFalse_ReturnsEmptyString()
    {
        var extension = MimeTypeMap.GetExtension("application/x-fclex-unknown", false);

        Assert.Equal(string.Empty, extension);
    }

    [Fact]
    public void GetExtension_WhenMimeTypeLooksLikeExtension_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => MimeTypeMap.GetExtension(".json"));

        Assert.Contains("not valid", ex.Message);
    }

    [Fact]
    public void TryGetExtension_WhenMimeTypeIsUnknown_ReturnsFalseAndNullExtension()
    {
        var found = MimeTypeMap.TryGetExtension("application/x-fclex-unknown", out var extension);

        Assert.False(found);
        Assert.Null(extension);
    }
}
