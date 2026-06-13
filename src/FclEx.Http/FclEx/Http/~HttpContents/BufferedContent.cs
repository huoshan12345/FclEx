namespace FclEx.Http;

/// <summary>
/// <see cref="HttpContent"/> implementation backed by an in-memory byte buffer.
/// It is used to make request content reusable across retries and redirects.
/// </summary>
public class BufferedContent : HttpContent
{
    private bool _disposed;
    private readonly byte[] _buffer;

    private BufferedContent(byte[] buffer)
    {
        _buffer = buffer;
    }

    /// <summary>
    /// Returns this instance while it has not been disposed, otherwise returns a clone that shares the same byte buffer and headers.
    /// This allows request-building code to reuse buffered content until <see cref="HttpContent"/> disposal makes a fresh instance necessary.
    /// </summary>
    public BufferedContent CloneIfDisposed()
    {
        return _disposed
            ? Clone()
            : this;
    }

    /// <summary>
    /// Creates another <see cref="BufferedContent"/> over the same immutable byte buffer and copies the content headers.
    /// </summary>
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

    /// <summary>
    /// Creates buffered content by reading the inner content into memory.
    /// Content headers are copied except Content-Length, which is recomputed from the buffer length.
    /// </summary>
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
