namespace FclEx.Http;

/// <summary>
/// Helpers for reading, buffering, and wrapping <see cref="HttpContent"/>.
/// Several methods provide cancellation or timeout-aware behavior on target frameworks where the BCL API is missing it.
/// </summary>
public static class HttpContentExtensions
{
#if !NET5_0_OR_GREATER
    /// <summary>
    /// Compatibility overload that accepts a cancellation token on target frameworks where <see cref="HttpContent.ReadAsStreamAsync()"/> has no token overload.
    /// The token can cancel the returned task before or during the async operation only where the underlying framework cooperates.
    /// </summary>
    public static async Task<Stream> ReadAsStreamAsync(this HttpContent content, CancellationToken token)
    {
        // NOTE: do not call ReadAsStreamAsync(this HttpContent content, int bufferSize, TimeSpan? readBufferTimeout, CancellationToken token)
        // to avoid circular call.
        return await content.ReadAsStreamAsync();
    }

    /// <summary>
    /// Compatibility overload that reads content as a string with a cancellation token.
    /// The content is first read through the timeout-aware stream helper used by this package.
    /// </summary>
    public static async Task<string> ReadAsStringAsync(this HttpContent content, CancellationToken token)
    {
        using var stream = await content.ReadAsStreamAsync(null, null, token);
        using var sr = new StreamReader(stream, true);
        return await sr.ReadToEndAsync();
    }

    /// <summary>
    /// Compatibility overload that reads content as bytes with a cancellation token.
    /// </summary>
    public static Task<byte[]> ReadAsByteArrayAsync(this HttpContent content, CancellationToken token)
    {
        return content.ReadAsByteArrayAsync(null, null, token);
    }
#endif

    /// <summary>
    /// Reads content into a seekable <see cref="MemoryStream"/>.
    /// The method preallocates from Content-Length when available and throws when that length exceeds <see cref="int.MaxValue"/>.
    /// </summary>
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

    /// <summary>
    /// Reads content into a byte array using the optional copy buffer size and read timeout.
    /// </summary>
    public static async Task<byte[]> ReadAsByteArrayAsync(this HttpContent content, int? bufferSize, TimeSpan? readBufferTimeout, CancellationToken token)
    {
#if NET5_0_OR_GREATER
        await
#endif
        using var ms = await content.ReadAsStreamAsync(bufferSize, readBufferTimeout, token);
        return ms.ToArray();
    }

    /// <summary>
    /// Wraps content so it is GZip-compressed while being serialized.
    /// Disposing the returned wrapper also disposes the original content.
    /// </summary>
    public static GZipContent ToGZip(this HttpContent content, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int? bufferSize = null, CancellationToken token = default)
        => new(content, compressionLevel, timeout, bufferSize, token);

#if NET5_0_OR_GREATER
    /// <summary>
    /// Wraps content so it is Brotli-compressed while being serialized.
    /// Disposing the returned wrapper also disposes the original content.
    /// </summary>
    public static BrotliContent ToBrotli(this HttpContent content, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int? bufferSize = 256 * 1024, CancellationToken token = default)
        => new(content, compressionLevel, timeout, bufferSize, token);
#endif

    /// <summary>
    /// Wraps content so it is deflate-compressed while being serialized.
    /// Disposing the returned wrapper also disposes the original content.
    /// </summary>
    public static DeflateContent ToDeflate(this HttpContent content, CompressionLevel compressionLevel = CompressionLevel.Optimal,
        TimeSpan? timeout = null, int? bufferSize = null, CancellationToken token = default)
        => new(content, compressionLevel, timeout, bufferSize, token);

    /// <summary>
    /// Applies the selected compression wrapper to content.
    /// <see cref="CompressionMethod.None"/> returns the original content instance unchanged.
    /// </summary>
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

    /// <summary>
    /// Converts content to reusable in-memory <see cref="BufferedContent"/>.
    /// Existing <see cref="BufferedContent"/> is returned unchanged; <see langword="null"/> content remains <see langword="null"/>.
    /// </summary>
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
        /// <summary>
        /// Creates UTF-8 JSON string content from an existing JSON payload.
        /// </summary>
        public static HttpContent FromJson(string json)
        {
            return new StringContent(json, Encoding.UTF8, MediaTypes.Json);
        }

        /// <summary>
        /// Serializes an object to JSON and returns UTF-8 JSON string content.
        /// </summary>
        public static HttpContent Json<T>(T obj, JsonSerializerOptions? options = null)
        {
            var json = obj.ToJson(options);
            return new StringContent(json, Encoding.UTF8, MediaTypes.Json);
        }
    }
}
