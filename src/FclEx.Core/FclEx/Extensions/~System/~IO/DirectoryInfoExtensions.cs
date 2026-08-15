namespace FclEx.Extensions;

public static class DirectoryInfoExtensions
{
    public static DirectoryInfo TryCreate(this DirectoryInfo dir)
    {
        if (dir.Exists == false)
        {
            dir.Create();
            dir.Refresh();
        }
        return dir;
    }

    public static DirectoryInfo TryDelete(this DirectoryInfo dir, bool recursive = false)
    {
        if (dir.Exists)
        {
            dir.Delete(recursive);
            dir.Refresh();
        }
        return dir;
    }

    /// <summary>
    /// Deletes the directory and all of its contents if it exists, then creates an empty directory at the same path.
    /// </summary>
    /// <exception cref="InvalidOperationException">The directory represents a file-system root.</exception>
    public static DirectoryInfo Recreate(this DirectoryInfo dir)
    {
        Check.NotNull(dir);
        if (dir.Parent is null)
            throw new InvalidOperationException("A file-system root cannot be recreated.");

        dir.TryDelete(true);
        dir.Create();
        dir.Refresh();
        return dir;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DirectoryInfo Sub(this DirectoryInfo dir, string name)
    {
        Check.NotNull(dir);
        ValidateDirectChildName(name);
        return new(Path.Combine(dir.FullName, name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FileInfo File(this DirectoryInfo dir, string name)
    {
        Check.NotNull(dir);
        ValidateDirectChildName(name);
        return new FileInfo(Path.Combine(dir.FullName, name));
    }

    /// <summary>
    /// Determines whether <paramref name="directory"/> is lexically below <paramref name="parent"/>.
    /// </summary>
    /// <remarks>
    /// The directory itself is not considered its own descendant. Symbolic links are not resolved.
    /// Path comparison follows the current operating system: case-insensitive on Windows and case-sensitive elsewhere.
    /// </remarks>
    public static bool IsDescendantOf(this DirectoryInfo directory, DirectoryInfo parent)
    {
        Check.NotNull(directory);
        Check.NotNull(parent);

        var directoryPath = Path.GetFullPath(directory.FullName);
        var parentPath = Path.GetFullPath(parent.FullName);
        var comparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (string.Equals(directoryPath, parentPath, comparison))
            return false;

        if (parentPath.EndsWith(Path.DirectorySeparatorChar) == false
            && parentPath.EndsWith(Path.AltDirectorySeparatorChar) == false)
        {
            parentPath += Path.DirectorySeparatorChar;
        }

        return directoryPath.StartsWith(parentPath, comparison);
    }

    private static void ValidateDirectChildName(string name)
    {
        Check.NotEmpty(name);

        if (Path.IsPathRooted(name)
            || name is "." or ".."
            || name.IndexOf(Path.DirectorySeparatorChar) >= 0
            || name.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
        {
            throw new ArgumentException("The name must identify one direct child and cannot contain a path.", nameof(name));
        }
    }

    public static bool IsEmpty(this DirectoryInfo dir)
    {
        return dir.EnumerateFileSystemInfos().Any() == false;
    }

    public static DirectoryInfo Rename(this DirectoryInfo dir, string name)
    {
        Check.NotNull(dir);
        Check.NotEmpty(name);

        if (dir.Name == name)
            return dir;

        var parent = dir.Parent;
        Check.NotNull(parent);

        var newName = Path.Combine(parent.FullName, name);
        dir.MoveTo(newName);
        return new DirectoryInfo(newName);
    }
}
