namespace FclEx.Extensions;

public static class StreamExtensions
{
    private const int DefaultBufferSize = 256 * 1024;  // Byte buffer size

    public static byte[] ReadAllBytes(this Stream stream, int bufferSize = DefaultBufferSize)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream, bufferSize);
        return memoryStream.ToArray();
    }

    public static async Task<byte[]> ReadAllBytesAsync(this Stream stream,
        int bufferSize = DefaultBufferSize,
        TimeSpan? readBufferTimeout = null,
        CancellationToken token = default)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, bufferSize, readBufferTimeout, token);
        return memoryStream.ToArray();
    }

    public static async Task<string> ReadAllTextAsync(this Stream stream,
        Encoding? encoding = null,
        bool detectEncodingFromByteOrderMarks = true,
        int bufferSize = DefaultBufferSize,
        bool leaveOpen = false,
        CancellationToken token = default)
    {
        using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen);
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

    public static async Task CopyToAsync(this Stream source,
        Stream dest, int bufferSize = DefaultBufferSize,
        TimeSpan? readBufferTimeout = null,
        CancellationToken token = default)
    {
        using var disposable = ArrayPool<byte>.Shared.GetPooled(bufferSize);
        var buffer = disposable.Value;

        while (true)
        {
            using var cts = token.WithTimeout(readBufferTimeout);
            var bytesCopied = await source.ReadAsync(buffer, 0, buffer.Length, cts.Token);
            if (bytesCopied <= 0)
                break;

            await dest.WriteAsync(buffer, 0, bytesCopied, cts.Token);
        }
    }
}