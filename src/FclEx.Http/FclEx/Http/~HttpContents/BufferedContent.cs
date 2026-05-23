namespace FclEx.Http;

public class BufferedContent : HttpContent
{
    public HttpContent Content { get; }
    public int BufferSize { get; }
    public TimeSpan? Timeout { get; }
    public CancellationToken Token { get; }

    protected static readonly MethodInfo MethodOfTryComputeLength
        = typeof(HttpContent).GetRequiredMethod(nameof(TryComputeLength));

    public BufferedContent(HttpContent content, TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken token = default)
    {
        Content = content;
        Timeout = timeout;
        Token = token;
        BufferSize = bufferSize;
        content.Headers.CopyTo(Headers);
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
#if NET6_0_OR_GREATER
        await
#endif
        using var contentStream = await Content.ReadAsStreamAsync(Token);
        await contentStream.CopyToAsync(stream, BufferSize, Timeout, Token);
    }

    protected override bool TryComputeLength(out long length)
    {
        var paras = new object?[] { null };
        var result = MethodOfTryComputeLength.InvokeInstance<bool>(Content, paras);
        length = paras[0].CastTo<long>();
        return result;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Content.Dispose();
        }
        base.Dispose(disposing);
    }
}