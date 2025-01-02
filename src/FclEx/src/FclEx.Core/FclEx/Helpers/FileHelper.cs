namespace FclEx.Helpers;

public static class FileHelper
{
    private static readonly Regex FileNumberSuffix = new(@"(?<=_)\d*$", RegexOptions.Compiled);

    public static string GetNewFileName(string fileName)
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

    public static bool AreSame(FileInfo f1, FileInfo f2)
    {
        if (f1.LastWriteTimeUtc != f2.LastWriteTimeUtc)
            return false;

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
