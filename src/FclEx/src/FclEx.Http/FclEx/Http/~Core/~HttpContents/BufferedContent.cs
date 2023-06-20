namespace FclEx.Http;

public class BufferedContent : HttpContent
{
    protected readonly HttpContent _content;
    protected readonly int _bufferSize;
    protected readonly TimeSpan? _timeout;
    protected readonly CancellationToken _token;

    protected static readonly MethodInfo _methodOfTryComputeLength
        = typeof(HttpContent).GetRequiredMethod(nameof(TryComputeLength));

    public BufferedContent(HttpContent content, TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken token = default)
    {
        _content = content;
        _timeout = timeout;
        _token = token;
        _bufferSize = bufferSize;

        content.Headers.CopyTo(Headers);
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        await using var contentStream = await _content.ReadAsStreamAsync(_token);
        await contentStream.CopyToAsync(stream, _bufferSize, _timeout, _token);
    }

    protected override bool TryComputeLength(out long length)
    {
        var paras = new object?[] { null };
        var result = _methodOfTryComputeLength.InvokeInstance<bool>(_content, paras);
        length = paras[0].CastTo<long>();
        return result;
    }
}