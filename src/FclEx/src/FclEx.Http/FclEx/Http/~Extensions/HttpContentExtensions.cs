namespace FclEx.Http;

public static class HttpContentExtensions
{
    public static async Task<MemoryStream> ReadAsStreamAsync(this HttpContent content, int bufferSize, TimeSpan? readBufferTimeout, CancellationToken token)
    {
        var len = content.Headers.ContentLength ?? 0;
        var ms = new MemoryStream((int)len);
        await using (var stream = await content.ReadAsStreamAsync(token).IgnoreSyncContext())
            await stream.CopyToAsync(ms, bufferSize, readBufferTimeout, token);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public static async Task<byte[]> ReadAsByteArrayAsync(this HttpContent content, int bufferSize, TimeSpan? readBufferTimeout, CancellationToken token)
    {
        await using var ms = await content.ReadAsStreamAsync(bufferSize, readBufferTimeout, token);
        return ms.ToArray();
    }

    public static BufferedContent ToBuffered(this HttpContent content, TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken token = default)
        => new(content, timeout, bufferSize, token);

    public static GZipContent ToGZip(this HttpContent content, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken token = default)
        => new(content, compressionLevel, timeout, bufferSize, token);

    public static BrotliContent ToBrotli(this HttpContent content, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken token = default)
        => new(content, compressionLevel, timeout, bufferSize, token);

    public static DeflateContent ToDeflate(this HttpContent content, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken token = default)
        => new(content, compressionLevel, timeout, bufferSize, token);

    public static HttpContent ToCompressed(this HttpContent content, CompressionMethod compressionMethod, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken token = default)
    {
        return compressionMethod switch
        {
            CompressionMethod.None => content.ToBuffered(timeout, bufferSize, token),
            CompressionMethod.GZip => content.ToGZip(compressionLevel, timeout, bufferSize, token),
            CompressionMethod.Deflate => content.ToDeflate(compressionLevel, timeout, bufferSize, token),
            CompressionMethod.Brotli => content.ToBrotli(compressionLevel, timeout, bufferSize, token),
            _ => throw new ArgumentOutOfRangeException(nameof(compressionMethod), compressionMethod, null)
        };
    }
}