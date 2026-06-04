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
        if (!_disposed)
            return this;

        var content = new BufferedContent(_buffer);
        return content;
    }

    public static async Task<BufferedContent> CreateAsync(HttpContent inner, TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken cancellationToken = default)
    {
        var buffer = await inner
            .ReadAsByteArrayAsync(bufferSize, timeout, cancellationToken)
            .ConfigureAwait(false);

        var content = new BufferedContent(buffer);
        foreach (var header in inner.Headers)
        {
            content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
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
