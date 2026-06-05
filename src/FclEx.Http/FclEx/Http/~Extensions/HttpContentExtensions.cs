namespace FclEx.Http;

public static class HttpContentExtensions
{
#if !NET5_0_OR_GREATER
    public static async Task<Stream> ReadAsStreamAsync(this HttpContent content, CancellationToken token)
    {
        // NOTE: do not call ReadAsStreamAsync(this HttpContent content, int bufferSize, TimeSpan? readBufferTimeout, CancellationToken token)
        // to avoid circular call.
        return await content.ReadAsStreamAsync();
    }

    public static async Task<string> ReadAsStringAsync(this HttpContent content, CancellationToken token)
    {
        using var stream = await content.ReadAsStreamAsync(null, null, token);
        using var sr = new StreamReader(stream, true);
        return await sr.ReadToEndAsync();
    }

    public static Task<byte[]> ReadAsByteArrayAsync(this HttpContent content, CancellationToken token)
    {
        return content.ReadAsByteArrayAsync(null, null, token);
    }
#endif

    public static async Task<MemoryStream> ReadAsStreamAsync(this HttpContent content, int? bufferSize, TimeSpan? readBufferTimeout, CancellationToken token)
    {
        var len = content.Headers.ContentLength ?? 0;
        if (len > int.MaxValue)
            throw new InvalidOperationException("Content length is too large.");

        var ms = new MemoryStream((int)len);
#if NET5_0_OR_GREATER
        await
#endif
        using (var stream = await content.ReadAsStreamAsync(token))
        {
            await stream.CopyToAsync(ms, bufferSize, readBufferTimeout, token);
        }
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public static async Task<byte[]> ReadAsByteArrayAsync(this HttpContent content, int? bufferSize, TimeSpan? readBufferTimeout, CancellationToken token)
    {
#if NET5_0_OR_GREATER
        await
#endif
        using var ms = await content.ReadAsStreamAsync(bufferSize, readBufferTimeout, token);
        return ms.ToArray();
    }

    public static GZipContent ToGZip(this HttpContent content, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int? bufferSize = null, CancellationToken token = default)
        => new(content, compressionLevel, timeout, bufferSize, token);

#if NET5_0_OR_GREATER
    public static BrotliContent ToBrotli(this HttpContent content, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int? bufferSize = 256 * 1024, CancellationToken token = default)
        => new(content, compressionLevel, timeout, bufferSize, token);
#endif

    public static DeflateContent ToDeflate(this HttpContent content, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int? bufferSize = null, CancellationToken token = default)
        => new(content, compressionLevel, timeout, bufferSize, token);

    public static HttpContent ToCompressed(this HttpContent content, CompressionMethod compressionMethod, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int ?bufferSize = null, CancellationToken token = default)
    {
        return compressionMethod switch
        {
            CompressionMethod.None => content,
            CompressionMethod.GZip => content.ToGZip(compressionLevel, timeout, bufferSize, token),
            CompressionMethod.Deflate => content.ToDeflate(compressionLevel, timeout, bufferSize, token),
#if NET6_0_OR_GREATER
            CompressionMethod.Brotli => content.ToBrotli(compressionLevel, timeout, bufferSize, token),
#endif
            _ => throw new ArgumentOutOfRangeException(nameof(compressionMethod), compressionMethod, null)
        };
    }

    public static async Task<BufferedContent?> ToBufferedContentAsync(this HttpContent? content,
        TimeSpan? timeout = null, int? bufferSize = null, CancellationToken token = default)
    {
        return content switch
        {
            null => null,
            BufferedContent bufferedContent => bufferedContent,
            _ => await BufferedContent.CreateAsync(content, timeout, bufferSize, token),
        };
    }

    extension(HttpContent)
    {
        public static HttpContent FromJson(string json)
        {
            return new StringContent(json, Encoding.UTF8, MediaTypes.Json);
        }

        public static HttpContent Json<T>(T obj, JsonSerializerOptions? options = null)
        {
            var json = obj.ToJson(options);
            return new StringContent(json, Encoding.UTF8, MediaTypes.Json);
        }
    }
}