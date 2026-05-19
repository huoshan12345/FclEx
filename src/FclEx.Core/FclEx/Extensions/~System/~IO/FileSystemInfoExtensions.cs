namespace FclEx.Extensions;

public static class FileSystemInfoExtensions
{
    public static readonly char DirectorySeparatorChar = Path.DirectorySeparatorChar;
    public static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

    /// <summary>
    /// Checks if the file or directory has the <see cref="FileAttributes.Hidden"/> attribute.
    /// </summary>
    public static bool IsHidden(this FileSystemInfo info)
    {
        return info.Attributes.HasFlag(FileAttributes.Hidden);
    }

    /// <summary>
    /// Gets the depth level of the current path relative to the root. <br/>
    /// Examples: <br/>
    /// <example>
    /// C:\ -> 0 (for DirectoryInfo) <br/>
    /// C:\foo\ -> 1 <br/>
    /// C:\foo\bar.txt -> 1
    /// </example>
    /// </summary>
    public static int GetDirectoryDepth(this FileSystemInfo info)
    {
        var path = info.GetRootRelativePath();
        var count = path.Count(m => m == DirectorySeparatorChar);
        // the path of directory always ends with a slash
        return info is DirectoryInfo
            ? count - 1
            : count;
    }

    /// <summary>
    /// Determines whether the current <see cref="FileSystemInfo"/> represents the root of a drive or network share.
    /// </summary>
    public static bool IsRoot(this FileSystemInfo info)
    {
        return info.FullName.Equals(info.GetRootPath(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the root portion of the path (e.g., "C:\" or "\\server\share\"). <br/>
    /// The result always ends with a <see cref="DirectorySeparatorChar"/>. <br/>
    /// Examples: <br/>
    /// <example>
    /// Windows: C:\foo -> C:\ <br/>
    /// Windows: \\server\share\foo -> \\server\share\ <br/>
    /// Unix: /home/user -> /
    /// </example>
    /// </summary>
    public static string GetRootPath(this FileSystemInfo info)
    {
        var root = Path.GetPathRoot(info.FullName);
        if (root.IsNullOrEmpty())
            return "";

        return root.EndsWith(DirectorySeparatorChar)
            ? root
            : root + DirectorySeparatorChar;
    }

    /// <summary>
    /// Gets the name of the first directory immediately following the root. <br/>
    /// Examples: <br/>
    /// <example>
    /// C:\foo\bar\ -> foo <br/>
    /// C:\foo\bar\text.txt -> foo
    /// </example>
    /// </summary>
    public static string GetTopLevelDirectoryName(this FileSystemInfo info)
    {
        var path = info.GetRootRelativePath();
        for (var i = 1; i < path.Length; i++)
        {
            if (path[i] == Path.DirectorySeparatorChar)
                return path[1..i];
        }
        return "";
    }

    /// <summary>
    /// Returns the path relative to the root, ensured to start with a separator. <br/>
    /// Directories will also end with a separator. <br/>
    /// Examples: <br/>
    /// <example>
    /// C:\ -> \ <br/>
    /// C:\foo\bar.txt -> \foo\bar.txt <br/>
    /// C:\foo\bar\ -> \foo\bar\
    /// </example>
    /// </summary>
    public static string GetRootRelativePath(this FileSystemInfo info)
    {
        var root = info.GetRootPath();
        var fullPath = info.FullName;

        // Since root now ends with a separator, 
        // we take the substring starting from the last character of the root.
        // Example: Full="C:\foo", Root="C:\", Substring starts at index 2 -> "\foo"
        var path = fullPath.Length >= root.Length
            ? fullPath[(root.Length - 1)..]
            : DirectorySeparator;

        // Basic cleanup: ensure it starts with a separator
        if (string.IsNullOrEmpty(path) || !path.StartsWith(DirectorySeparatorChar))
        {
            path = DirectorySeparatorChar + path;
        }

        // Ensure directories end with a separator
        if (info is DirectoryInfo && !path.EndsWith(DirectorySeparatorChar))
        {
            path += DirectorySeparatorChar;
        }

        return path;
    }
}