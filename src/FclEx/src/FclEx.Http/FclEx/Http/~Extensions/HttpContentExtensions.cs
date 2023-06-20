namespace FclEx.Http;

public static class HttpContentExtensions
{
    public static async Task<MemoryStream> ReadAsStreamAsync(this HttpContent content, int bufferSize, TimeSpan? readBufferTimeout, CancellationToken token)
    {
        var len = content.Headers.ContentLength ?? 0;
        var ms = new MemoryStream((int)len);
        await using (var stream = await content.ReadAsStreamAsync(token).DonotCapture())
            await stream.CopyToAsync(ms, bufferSize, readBufferTimeout, token);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public static async Task<byte[]> ReadAsByteArrayAsync(this HttpContent content, int bufferSize, TimeSpan? readBufferTimeout, CancellationToken token)
    {
        await using var ms = await ReadAsStreamAsync(content, bufferSize, readBufferTimeout, token);
        return ms.ToArray();
    }

    public static BufferedContent ToBuffered(this HttpContent content, TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken token = default)
        => new(content, timeout, bufferSize, token);

    public static GZipContent ToGZip(this HttpContent content, TimeSpan? timeout, int bufferSize = 256 * 1024, CancellationToken token = default)
        => new(content, timeout, bufferSize, token);

    public static HttpContent ToBuffered(this HttpContent content, bool useGZip, TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken token = default)
    {
        return useGZip 
            ? content.ToGZip(timeout, bufferSize, token) 
            : content.ToBuffered(timeout, bufferSize, token);
    }
}