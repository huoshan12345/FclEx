namespace FclEx.Extensions;

public static class FileExtensions
{
    internal const int DefaultBufferSize = 256 * 1024;

#if NET5_0_OR_GREATER
    private static int ReadChunk(Stream stream, Span<byte> buffer)
    {
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var read = stream.Read(buffer[totalRead..]);
            if (read == 0)
                break;

            totalRead += read;
        }

        return totalRead;
    }
#else
    private static int ReadChunk(Stream stream, byte[] buffer, int count)
    {
        var totalRead = 0;
        while (totalRead < count)
        {
            var read = stream.Read(buffer, totalRead, count - totalRead);
            if (read == 0)
                break;

            totalRead += read;
        }

        return totalRead;
    }
#endif

    extension(File)
    {
#if !NET5_0_OR_GREATER
        public static async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, DefaultBufferSize, useAsync: true);
            await fs.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
            await fs.FlushAsync(cancellationToken).NoCapture();
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
            => File.WriteAllTextAsync(path, content, Encoding.Utf8WithoutBom, token);

        public static async Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken token = default)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, useAsync: true);
            using var sr = new StreamReader(fs, encoding);
            return await sr.ReadToEndAsync(token);
        }

        public static Task<string> ReadAllTextAsync(string path, CancellationToken token = default)
            => File.ReadAllTextAsync(path, Encoding.UTF8, token);

        public static async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, useAsync: true);
            using var ms = new MemoryStream();
            await fs.CopyToAsync(ms, DefaultBufferSize, cancellationToken);
            return ms.ToArray();
        }
#endif
        /// <summary>
        /// Determines whether two files are identical by comparing their lengths
        /// and then reading and comparing their content in chunks.
        /// </summary>
        /// <param name="f1">The first file to compare.</param>
        /// <param name="f2">The second file to compare.</param>
        /// <returns><see langword="true"/> if both files exist, have the same length, and identical content; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either file is null.</exception>
        /// <exception cref="ArgumentException">Thrown if either file does not exist.</exception>
        public static bool AreFilesEqual(FileInfo f1, FileInfo f2)
        {
            Check.NotNull(f1);
            Check.NotNull(f2);
            Check.EqualTo(f1.Exists, true);
            Check.EqualTo(f2.Exists, true);

            var length = f1.Length;
            if (length != f2.Length)
                return false;

            using var stream1 = File.OpenRead(f1.FullName);
            using var stream2 = File.OpenRead(f2.FullName);

#if NET5_0_OR_GREATER
            Span<byte> buf1 = stackalloc byte[4096];
            Span<byte> buf2 = stackalloc byte[4096];
#else
            var buf1 = new byte[4096];
            var buf2 = new byte[4096];
#endif

            while (length > 0)
            {
                var toRead = (int)Math.Min(buf1.Length, length);
#if NET5_0_OR_GREATER
                var chunk1 = buf1[..toRead];
                var chunk2 = buf2[..toRead];
                var read1 = ReadChunk(stream1, chunk1);
                var read2 = ReadChunk(stream2, chunk2);
#else
                var read1 = ReadChunk(stream1, buf1, toRead);
                var read2 = ReadChunk(stream2, buf2, toRead);
#endif

                if (read1 != toRead || read2 != toRead)
                    return false;

#if NET5_0_OR_GREATER
                if (!chunk1.SequenceEqual(chunk2))
                    return false;
#else
                for (var i = 0; i < toRead; i++)
                {
                    if (buf1[i] != buf2[i])
                        return false;
                }
#endif

                length -= toRead;
            }

            return true;
        }

    }
}
