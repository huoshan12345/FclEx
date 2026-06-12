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
    public void Constructor_CopiesSourceHeadersExceptLengthAndExistingEncoding()
    {
        using var source = new StringContent("payload", Encoding.UTF8, MediaTypes.Json);
        source.Headers.ContentLanguage.Add("en-US");
        source.Headers.ContentEncoding.Add("identity");
        source.Headers.ContentLength = 999;

        using var content = source.ToGZip();

        Assert.Equal(MediaTypes.Json, content.Headers.ContentType?.MediaType);
        Assert.Equal(Encoding.UTF8.WebName, content.Headers.ContentType?.CharSet);
        Assert.Contains("en-US", content.Headers.ContentLanguage);
        Assert.DoesNotContain("identity", content.Headers.ContentEncoding);
        Assert.Contains("gzip", content.Headers.ContentEncoding);
        Assert.Null(content.Headers.ContentLength);
    }

    [Fact]
    public void ToCompressed_WhenCompressionMethodIsNone_ReturnsOriginalContent()
    {
        using var source = new StringContent("payload");

        var content = source.ToCompressed(CompressionMethod.None);

        Assert.Same(source, content);
    }

    [Fact]
    public async Task CopyToAsync_DisposesSourceReadStream()
    {
        var stream = new TrackingMemoryStream(Encoding.UTF8.GetBytes("payload"));
        using var source = new StreamContentForTest(stream);
        using var content = source.ToGZip();
        using var destination = new MemoryStream();

        await content.CopyToAsync(destination);

        Assert.True(stream.IsDisposed);
    }

    [Fact]
    public async Task CopyToAsync_WritesReadableGZipPayload()
    {
        using var source = new StringContent("payload", Encoding.UTF8, MediaTypes.Text);
        using var content = source.ToGZip();
        using var destination = new MemoryStream();

        await content.CopyToAsync(destination);

        destination.Position = 0;
        using var gzipStream = new GZipStream(destination, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream, Encoding.UTF8);
        Assert.Equal("payload", await reader.ReadToEndAsync());
    }

    [Fact]
    public void Dispose_DisposesWrappedContent()
    {
        var source = new DisposableContent();
        using var content = source.ToGZip();

        content.Dispose();

        Assert.True(source.IsDisposed);
    }

    private sealed class StreamContentForTest(TrackingMemoryStream stream) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            throw new NotSupportedException();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = stream.Length;
            return true;
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return Task.FromResult<Stream>(stream);
        }
    }

    private sealed class TrackingMemoryStream(byte[] buffer) : MemoryStream(buffer)
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                IsDisposed = true;

            base.Dispose(disposing);
        }
    }

    private sealed class DisposableContent : StringContent
    {
        public DisposableContent()
            : base("payload")
        {
        }

        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                IsDisposed = true;

            base.Dispose(disposing);
        }
    }
}
