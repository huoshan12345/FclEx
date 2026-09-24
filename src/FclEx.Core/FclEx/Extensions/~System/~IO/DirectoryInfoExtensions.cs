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

    [MethodImpl(AggressiveInlining)]
    public static DirectoryInfo Sub(this DirectoryInfo dir, string name)
    {
        Check.NotNull(dir);
        ValidateDirectChildName(name);
        return new(Path.Combine(dir.FullName, name));
    }

    /// <summary>
    /// Gets a directory beneath <paramref name="dir"/> by combining the supplied direct-child names in order.
    /// </summary>
    /// <param name="dir">The directory from which to start.</param>
    /// <param name="names">Zero or more names, each identifying a direct child directory.</param>
    /// <returns>A <see cref="DirectoryInfo"/> for the resulting path. With no names, returns the directory path itself.</returns>
    /// <remarks>The returned object represents a path; this method does not create the directory or check whether it exists.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="dir"/> or <paramref name="names"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">A name is empty, rooted, a dot segment, or contains a directory separator.</exception>
    public static DirectoryInfo Sub(this DirectoryInfo dir, params IEnumerable<string> names)
    {
        Check.NotNull(dir);
        Check.NotNull(names);

        var builder = new PathBuilder(dir.FullName);
        foreach (var name in names)
        {
            ValidateDirectChildName(name);
            builder.Add(name);
        }

        var path = builder.Build();
        return new DirectoryInfo(path);
    }

    [MethodImpl(AggressiveInlining)]
    public static FileInfo File(this DirectoryInfo dir, string name)
    {
        Check.NotNull(dir);
        ValidateDirectChildName(name);
        return new FileInfo(Path.Combine(dir.FullName, name));
    }

    /// <summary>
    /// Gets a file beneath <paramref name="dir"/> by combining the supplied direct-child names in order.
    /// </summary>
    /// <param name="dir">The directory from which to start.</param>
    /// <param name="names">One or more names; the last identifies the file and any preceding names identify directories.</param>
    /// <returns>A <see cref="FileInfo"/> for the resulting path.</returns>
    /// <remarks>The returned object represents a path; this method does not create the file or check whether it exists.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="dir"/> or <paramref name="names"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">No names are supplied, or a name is empty, rooted, a dot segment, or contains a directory separator.</exception>
    public static FileInfo File(this DirectoryInfo dir, params IEnumerable<string> names)
    {
        Check.NotNull(dir);
        Check.NotNull(names);

        var builder = new PathBuilder(dir.FullName);
        var hasName = false;
        foreach (var name in names)
        {
            ValidateDirectChildName(name);
            builder.Add(name);
            hasName = true;
        }

        if (hasName == false)
            throw new ArgumentException("At least one name is required to identify a file.", nameof(names));

        var path = builder.Build();
        return new FileInfo(path);
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
            || name.Contains(Path.DirectorySeparatorChar)
            || name.Contains(Path.AltDirectorySeparatorChar))
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
        ValidateDirectChildName(name);

        if (dir.Name == name)
            return dir;

        var parent = dir.Parent;
        Check.NotNull(parent);

        var newName = Path.Combine(parent.FullName, name);
        dir.MoveTo(newName);
        return new DirectoryInfo(newName);
    }
}
