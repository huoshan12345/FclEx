namespace FclEx.Extensions;

public static class FileExtensions
{
    internal const int DefaultBufferSize = 256 * 1024;

    extension(File)
    {
#if !NET5_0_OR_GREATER
        public static async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, DefaultBufferSize, useAsync: true);
            await fs.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
            await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task WriteAllTextAsync(string path, string content, Encoding encoding, CancellationToken token = default)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, DefaultBufferSize, useAsync: true);
            using var sw = new StreamWriter(fs, encoding);

            await sw.WriteAsync(content);
            await sw.FlushAsync(token);
            await fs.FlushAsync(token);
        }

        public static Task WriteAllTextAsync(string path, string content, CancellationToken token = default)
            => File.WriteAllTextAsync(path, content, Encoding.UTF8, token);

        public static async Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken token = default)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, useAsync: true);
            using var sr = new StreamReader(fs, encoding);
            return await sr.ReadToEndAsync();
        }

        public static Task<string> ReadAllTextAsync(string path, CancellationToken token = default)
            => File.ReadAllTextAsync(path, Encoding.UTF8, token);
#endif
    }
}
