namespace FclEx.Extensions;

public static class FileExtensions
{
    internal const int DefaultBufferSize = 256 * 1024;

    extension(File)
    {
#if NETSTANDARD2_0
        public static async Task WriteAllTextAsync(string path, string content, Encoding? encoding = null, CancellationToken token = default)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, DefaultBufferSize, true);
            using var sw = new StreamWriter(fs, encoding ?? Encoding.UTF8);

            await sw.WriteAsync(content);
            await sw.FlushAsync(token);
            await fs.FlushAsync(token);
        }

        public static Task WriteAllTextAsync(string path, string content, CancellationToken token = default)
            => File.WriteAllTextAsync(path, content, null, token);

        public static async Task<string> ReadAllTextAsync(string path, Encoding? encoding = null, CancellationToken token = default)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, true);
            using var sr = new StreamReader(fs, encoding ?? Encoding.UTF8);
            return await sr.ReadToEndAsync();
        }

        public static Task<string> ReadAllTextAsync(string path, CancellationToken token = default)
            => File.ReadAllTextAsync(path, null, token);
#endif
    }
}
