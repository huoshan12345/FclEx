namespace FclEx.Extensions;

public static class StreamExtensions
{
    private const int DefaultBufferSize = 256 * 1024;  // Byte buffer size

    public static byte[] ReadAllBytes(this Stream stream, int? bufferSize = null)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream, bufferSize ?? DefaultBufferSize);
        return memoryStream.ToArray();
    }

    public static async Task<byte[]> ReadAllBytesAsync(this Stream stream,
        int? bufferSize = null,
        TimeSpan? bufferTransferTimeout = null,
        CancellationToken token = default)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, bufferSize ?? DefaultBufferSize, bufferTransferTimeout, token);
        return memoryStream.ToArray();
    }

    /// <summary>Asynchronously reads all remaining text from a stream.</summary>
    /// <param name="stream">The stream to read.</param>
    /// <param name="encoding">The text encoding, or UTF-8 when <see langword="null"/>.</param>
    /// <param name="detectEncodingFromByteOrderMarks">Whether a byte-order mark can override <paramref name="encoding"/>.</param>
    /// <param name="bufferSize">The reader buffer size, or the library default when <see langword="null"/>.</param>
    /// <param name="leaveOpen">Whether to leave <paramref name="stream"/> open after reading. The default is <see langword="true"/>.</param>
    /// <param name="token">The cancellation token for the read operation.</param>
    /// <returns>The text read from the stream's current position to its end.</returns>
    public static async Task<string> ReadAllTextAsync(this Stream stream,
        Encoding? encoding = null,
        bool detectEncodingFromByteOrderMarks = true,
        int? bufferSize = null,
        bool leaveOpen = true,
        CancellationToken token = default)
    {
        using var reader = new StreamReader(stream, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks, bufferSize ?? DefaultBufferSize, leaveOpen);
        var text = await reader.ReadToEndAsync(token);
        return text;
    }

    public static Stream SeekToBegin(this Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public static void Write(this Stream stream, byte[] bytes) => stream.Write(bytes, 0, bytes.Length);

    public static Task WriteAsync(this Stream stream, byte[] bytes) => stream.WriteAsync(bytes, 0, bytes.Length);

    public static async Task CopyToAsync(
        this Stream source,
        Stream dest,
        int? bufferSize = null,
        TimeSpan? bufferTransferTimeout = null,
        CancellationToken token = default)
    {
        using var disposable = ArrayPool<byte>.Shared.GetPooled(bufferSize ?? DefaultBufferSize);
        var buffer = disposable.Value;

        while (true)
        {
            using var cts = token.WithTimeout(bufferTransferTimeout);
            var bytesCopied = await source.ReadAsync(buffer, 0, buffer.Length, cts.Token);
            if (bytesCopied <= 0)
                break;

            await dest.WriteAsync(buffer, 0, bytesCopied, cts.Token);
        }
    }
}
