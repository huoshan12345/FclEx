namespace FclEx.Http.HttpContents;

public class CompressedContentTests
{
    [Theory]
    [InlineData(CompressionMethod.GZip, "gzip")]
    [InlineData(CompressionMethod.Deflate, "deflate")]
#if NET6_0_OR_GREATER
    [InlineData(CompressionMethod.Brotli, "br")]
#endif
    public void ToCompressed_AddsContentEncodingHeader(CompressionMethod method, string expectedEncoding)
    {
        using var source = new StringContent("payload", Encoding.UTF8, MediaTypes.Text);
        using var content = source.ToCompressed(method);

        Assert.Contains(expectedEncoding, content.Headers.ContentEncoding);
    }

    [Fact]
    public void ToCompressed_WhenCompressionMethodIsNone_ReturnsOriginalContent()
    {
        using var source = new StringContent("payload");

        var content = source.ToCompressed(CompressionMethod.None);

        Assert.Same(source, content);
    }
}
