namespace FclEx.Http.Core;

public class HttpResponseStreamTests
{
    [Fact]
    public void Constructor_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        using var stream = new MemoryStream();

        var ex = Assert.Throws<ArgumentNullException>(() => new HttpResponseStream(null!, stream));

        Assert.Equal("response", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenStreamIsNull_ThrowsArgumentNullException()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);

        var ex = Assert.Throws<ArgumentNullException>(() => new HttpResponseStream(response, null!));

        Assert.Equal("stream", ex.ParamName);
    }

    [Fact]
    public void Properties_DelegateToInnerStream()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        using var inner = new MemoryStream([1, 2, 3]);
        using var stream = new HttpResponseStream(response, inner);

        Assert.True(stream.CanRead);
        Assert.True(stream.CanSeek);
        Assert.True(stream.CanWrite);
        Assert.Equal(3, stream.Length);

        stream.Position = 2;

        Assert.Equal(2, inner.Position);
        Assert.Equal(2, stream.Position);
    }

    [Fact]
    public void ReadWriteSeekSetLengthAndFlush_DelegateToInnerStream()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        using var inner = new TrackingMemoryStream();
        using var stream = new HttpResponseStream(response, inner);

        stream.Write([1, 2, 3, 4], 0, 4);
        stream.Seek(1, SeekOrigin.Begin);
        var buffer = new byte[2];
        var read = stream.Read(buffer, 0, buffer.Length);
        stream.SetLength(2);
        stream.Flush();

        Assert.Equal(2, read);
        Assert.Equal([2, 3], buffer);
        Assert.Equal(2, inner.Length);
        Assert.Equal(1, inner.FlushCount);
    }

    [Fact]
    public void Dispose_DisposesResponseAndInnerStream()
    {
        var content = new TrackingContent();
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = content,
        };
        var inner = new TrackingMemoryStream();
        var stream = new HttpResponseStream(response, inner);

        stream.Dispose();

        Assert.True(content.IsDisposed);
        Assert.True(inner.IsDisposed);
    }

    private sealed class TrackingMemoryStream : MemoryStream
    {
        public int FlushCount { get; private set; }

        public bool IsDisposed { get; private set; }

        public override void Flush()
        {
            FlushCount++;
            base.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                IsDisposed = true;

            base.Dispose(disposing);
        }
    }

    private sealed class TrackingContent : HttpContent
    {
        public bool IsDisposed { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return Task.CompletedTask;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                IsDisposed = true;

            base.Dispose(disposing);
        }
    }
}
