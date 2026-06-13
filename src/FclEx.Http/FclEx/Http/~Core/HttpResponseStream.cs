namespace FclEx.Http;

/// <summary>
/// Stream wrapper used for streaming HTTP responses.
/// Disposing the wrapper disposes both the response message and the underlying response stream.
/// </summary>
public class HttpResponseStream : Stream
{
    private readonly HttpResponseMessage _response;
    private readonly Stream _stream;

    /// <summary>
    /// Creates a stream wrapper that owns the supplied response message and stream.
    /// </summary>
    public HttpResponseStream(HttpResponseMessage response, Stream stream)
    {
        _response = Check.NotNull(response);
        _stream = Check.NotNull(stream);
    }

    /// <inheritdoc />
    public override void Flush() => _stream.Flush();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

    /// <inheritdoc />
    public override void SetLength(long value) => _stream.SetLength(value);

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

    /// <inheritdoc />
    public override bool CanRead => _stream.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => _stream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => _stream.CanWrite;

    /// <inheritdoc />
    public override long Length => _stream.Length;

    /// <inheritdoc />
    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing == false)
            return;

        base.Dispose(true);
        _response.Dispose();
        _stream.Dispose();
    }
}
