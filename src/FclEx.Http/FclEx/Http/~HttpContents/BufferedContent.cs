namespace FclEx.Http;

public class BufferedContent : HttpContent
{
    private bool _disposed;
    private readonly byte[] _buffer;

    private BufferedContent(byte[] buffer)
    {
        _buffer = buffer;
    }

    public BufferedContent CloneIfDisposed()
    {
        return _disposed
            ? Clone()
            : this;
    }

    public BufferedContent Clone()
    {
        var content = new BufferedContent(_buffer);
        CopyHeaders(Headers, content.Headers);
        return content;
    }

    protected static void CopyHeaders(HttpContentHeaders source, HttpContentHeaders destination)
    {
        // Remove Content-Length header to allow HttpContent to compute it based on the buffer length
        source.CopyTo(destination, HttpHeaderNames.ContentLength);
    }

    public static async Task<BufferedContent> CreateAsync(HttpContent inner, TimeSpan? timeout = null, int? bufferSize = null, CancellationToken cancellationToken = default)
    {
        var buffer = await inner
            .ReadAsByteArrayAsync(bufferSize, timeout, cancellationToken)
            .NoCapture();

        var content = new BufferedContent(buffer);
        CopyHeaders(inner.Headers, content.Headers);
        return content;
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        return stream.WriteAsync(_buffer, 0, _buffer.Length);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _buffer.Length;
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _disposed = true;

        base.Dispose(disposing);
    }
}
