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
    /// <returns><c>true</c> if both files exist, have the same length, and identical content; otherwise <c>false</c>.</returns>
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

#if NETSTANDARD2_0
        var buf1 = new byte[4096];
        var buf2 = new byte[4096];
#else
        Span<byte> buf1 = stackalloc byte[4096];
        Span<byte> buf2 = stackalloc byte[4096];
#endif

        // compare content for equality
        while (length > 0)
        {
            // figure out how much to read
            var toRead = buf1.Length;
            if (toRead > length)
                toRead = (int)length;

            length -= toRead;

            // read a chunk from each and compare

#if NETSTANDARD2_0
            var i = stream1.Read(buf1, 0, toRead);
            var j = stream2.Read(buf2, 0, toRead);
#else
            var i = stream1.Read(buf1);
            var j = stream2.Read(buf2);
#endif

            if (i != j)
                return false;

            if (buf1.SequenceEqual(buf2) == false)
                return false;
        }

        return true;
    }
}
