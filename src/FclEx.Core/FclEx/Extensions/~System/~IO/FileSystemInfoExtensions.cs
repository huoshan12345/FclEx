namespace FclEx.Extensions;

public static class FileSystemInfoExtensions
{
    public static bool IsHidden(this FileSystemInfo info)
    {
        return info.Attributes.HasFlag(FileAttributes.Hidden);
    }

    public static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

    public static int GetLevel(this FileSystemInfo info)
    {
        var path = info.GetFullPathWithoutRoot();
        var count = path.Count(m => m == Path.DirectorySeparatorChar);
        // the path of directory always ends with a slash
        return info is DirectoryInfo
            ? count - 1
            : count;
    }

    public static bool IsRoot(this FileSystemInfo info)
    {
        return info.GetFullPathWithoutRoot() == DirectorySeparator;
    }

    /// <summary>
    /// Get root dir (drive or network), which always does not end with <see cref="DirectorySeparator"/>. <br/>
    /// Examples: <br/>
    /// * C:\ -> C: <br/>
    /// * C:\foo\bar -> C: <br/>
    /// * C:\foo\bar\text.txt -> C: <br/>
    /// * \\network-machine\ -> \\network-machine <br/>
    /// * \\network-machine\foo\bar\ -> \\network-machine <br/>
    /// * \\network-machine\foo\bar\text.txt -> \\network-machine
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    [SuppressMessage("ReSharper", "CommentTypo")]
    public static string GetRoot(this FileSystemInfo info)
    {
        var path = info.FullName;

        if (path.Length < 2)
            return ""; // path without root

        // C:\foo\bar -> C:
        if (path[1] == Path.VolumeSeparatorChar)
        {
            return path[..2];
        }

        // network path
        // ReSharper disable once InvertIf
        if (path[0] == Path.DirectorySeparatorChar
            && path[1] == Path.DirectorySeparatorChar)
        {
            for (var i = 2; i < path.Length; i++)
            {
                // \\network-machine\foo\bar -> \\network-machine
                if (path[i] == Path.DirectorySeparatorChar)
                    return path[..i];
            }

            // \\network-machine -> \\network-machine
            return path;
        }

        return "";
    }

    /// <summary>
    /// The first dir name from the path. <br/>
    /// Example: <br/>
    /// C:\foo\bar\ -> foo <br/>
    /// C:\foo\bar\text.txt -> foo
    /// </summary>
    public static string GetFirstDir(this FileSystemInfo info)
    {
        var path = info.GetFullPathWithoutRoot();
        for (var i = 1; i < path.Length; i++)
        {
            if (path[i] == Path.DirectorySeparatorChar)
                return path.Substring(1, i - 1);
        }
        return "";
    }

    /// <summary>
    /// Get full path without root dir (drive or network), which always starts and ends with <see cref="DirectorySeparator"/>. <br/>
    /// Examples: <br/>
    /// * C:\ -> \ <br/>
    /// * C:\foo\bar\ -> \foo\bar\ <br/>
    /// * C:\foo\bar\text.txt -> \foo\bar\text.txt <br/>
    /// * \\network-machine\ -> \ <br/>
    /// * \\network-machine\foo\bar\ -> \foo\bar\ <br/>
    /// * \\network-machine\foo\bar\text.txt -> \foo\bar\text.txt
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    [SuppressMessage("ReSharper", "CommentTypo")]
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public static string GetFullPathWithoutRoot(this FileSystemInfo info)
    {
        // the path always starts with a slash
        // the path of directory always ends with a slash
        // C:\foo\bar -> \foo\bar\
        // \\network-machine\foo\bar -> \foo\bar\
        var root = info.GetRoot();
        var path = info.FullName.TrimStart(root);

        if (path == "")
            return DirectorySeparator;

        return info is DirectoryInfo && path.EndsWith(DirectorySeparator) == false
            ? path + DirectorySeparator
            : path;
    }
}