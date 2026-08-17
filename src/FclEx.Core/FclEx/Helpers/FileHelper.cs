namespace FclEx.Helpers;

public static class FileHelper
{
    private static readonly Regex FileNumberSuffix = new(@"(?<=_)\d*$", RegexOptions.Compiled);

    /// <summary>
    /// Generates a new file name by incrementing the numeric suffix.
    /// If the file name already ends with "_&lt;number&gt;", that number is incremented.
    /// Otherwise, "_1" is appended before the extension.
    /// </summary>
    /// <param name="fileName">The original file name (with or without extension).</param>
    /// <returns>A new file name with an incremented or added numeric suffix.</returns>
    public static string GetNextFileName(string fileName)
    {
        var dotIndex = fileName.LastIndexOf('.');

        var (name, ext) = dotIndex switch
        {
            >= 0 => (fileName[..dotIndex], fileName[dotIndex..]),
            _ => (fileName, string.Empty),
        };

        var newName = FileNumberSuffix.Replace(name, 0, v => v.ToInt() + 1, s => s + "_1");
        return newName + ext;
    }

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
}
