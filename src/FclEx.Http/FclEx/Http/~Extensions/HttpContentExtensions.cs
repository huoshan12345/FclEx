namespace FclEx.Http;

public static class HttpContentExtensions
{
#if NETSTANDARD2_0
    // ReSharper disable once UnusedParameter.Global
#pragma warning disable IDE0060 // Remove unused parameter
    public static Task<Stream> ReadAsStreamAsync(this HttpContent content, CancellationToken token)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        return content.ReadAsStreamAsync();
    }
#endif

    public static async Task<MemoryStream> ReadAsStreamAsync(this HttpContent content, int bufferSize, TimeSpan? readBufferTimeout, CancellationToken token)
    {
        var len = content.Headers.ContentLength ?? 0;
        var ms = new MemoryStream((int)len);
#if NET6_0_OR_GREATER
        await
#endif
        using (var stream = await content.ReadAsStreamAsync(token))
        {
            await stream.CopyToAsync(ms, bufferSize, readBufferTimeout, token);
        }
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public static async Task<byte[]> ReadAsByteArrayAsync(this HttpContent content, int bufferSize, TimeSpan? readBufferTimeout, CancellationToken token)
    {
#if NET6_0_OR_GREATER
        await
#endif
        using var ms = await content.ReadAsStreamAsync(bufferSize, readBufferTimeout, token);
        return ms.ToArray();
    }

    public static BufferedContent ToBuffered(this HttpContent content, TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken token = default)
        => new(content, timeout, bufferSize, token);

    public static GZipContent ToGZip(this HttpContent content, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken token = default)
        => new(content, compressionLevel, timeout, bufferSize, token);

#if NET6_0_OR_GREATER
    public static BrotliContent ToBrotli(this HttpContent content, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int bufferSize = 256 * 1024, CancellationToken token = default)
        => new(content, compressionLevel, timeout, bufferSize, token);    
#endif

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
#if NET6_0_OR_GREATER
            CompressionMethod.Brotli => content.ToBrotli(compressionLevel, timeout, bufferSize, token),
#endif
            _ => throw new ArgumentOutOfRangeException(nameof(compressionMethod), compressionMethod, null)
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