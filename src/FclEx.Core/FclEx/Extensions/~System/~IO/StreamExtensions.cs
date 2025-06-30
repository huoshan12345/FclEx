namespace FclEx.Extensions;

public static class StreamExtensions
{
    public static string ToString(this Stream stream, Encoding? encoding = null)
    {
        using var sr = new StreamReader(stream, encoding ?? Encoding.UTF8);
        return sr.ReadToEnd();
    }

    public static byte[] ToBytes(this Stream stream)
    {
        var bytes = new byte[stream.Length - stream.Position];
        _ = stream.Read(bytes, (int)stream.Position, bytes.Length);
        return bytes;
    }

    public static Stream SeekToBegin(this Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public static void Write(this Stream stream, byte[] bytes) => stream.Write(bytes, 0, bytes.Length);

    public static Task WriteAsync(this Stream stream, byte[] bytes) => stream.WriteAsync(bytes, 0, bytes.Length);

    public static async Task CopyToAsync(this Stream source, Stream dest, int bufferSize = 256 * 1024, TimeSpan? readBufferTimeout = null, CancellationToken token = default)
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