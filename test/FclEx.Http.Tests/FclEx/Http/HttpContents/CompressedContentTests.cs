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
}
