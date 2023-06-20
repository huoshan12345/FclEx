namespace FclEx.Http;

public class GZipContent : BufferedContent
{
    public GZipContent(HttpContent content, TimeSpan? timeout = null, int bufferSize = 262144, CancellationToken token = default) 
        : base(content, timeout, bufferSize, token)
    {
        Headers.Add(HttpKnownHeaderNames.ContentEncoding, "gzip");
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        await using var contentStream = await _content.ReadAsStreamAsync(_token);
        await using var gzipStream = new GZipStream(stream, CompressionMode.Compress, true);
        await contentStream.CopyToAsync(stream, _bufferSize, _timeout, _token);
    }
}