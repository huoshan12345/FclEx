namespace System.IO.Compression;

/// <summary>Describes one file or directory node in a ZIP archive tree.</summary>
public sealed class ZipArchiveEntryInfo
{
    /// <summary>Initializes information for an entry that physically exists in the archive.</summary>
    public ZipArchiveEntryInfo(ZipArchiveEntry entry)
        : this(
            Check.NotNull(entry),
            NormalizePath(entry.FullName),
            entry.FullName.EndsWith("/", StringComparison.Ordinal)
            || entry.FullName.EndsWith("\\", StringComparison.Ordinal))
    {
    }

    private ZipArchiveEntryInfo(ZipArchiveEntry? entry, string fullName, bool isDirectory)
    {
        if (fullName.Length == 0)
            throw new ArgumentException("A ZIP tree entry must have a non-empty path.", nameof(fullName));

        Entry = entry;
        FullName = fullName;
        Segments = Array.AsReadOnly(fullName.Split(['/'], StringSplitOptions.RemoveEmptyEntries));
        IsDirectory = isDirectory;
        Name = Segments[^1];

        var separatorIndex = fullName.LastIndexOf('/');
        Parent = separatorIndex < 0 ? string.Empty : fullName[..separatorIndex];
    }

    /// <summary>
    /// Gets the physical archive entry, or <see langword="null"/> when this directory was synthesized because the ZIP
    /// contains descendants but no explicit directory entry.
    /// </summary>
    public ZipArchiveEntry? Entry { get; }

    /// <summary>Gets the normalized full path, using <c>/</c> separators and no leading or trailing separator.</summary>
    public string FullName { get; }

    /// <summary>Gets the individual components of <see cref="FullName"/>.</summary>
    public IReadOnlyList<string> Segments { get; }

    /// <summary>Gets whether this node represents a directory.</summary>
    public bool IsDirectory { get; }

    /// <summary>Gets whether this node represents a file.</summary>
    public bool IsFile => !IsDirectory;

    /// <summary>Gets the final path component.</summary>
    public string Name { get; }

    /// <summary>Gets the normalized full path of the parent directory, or an empty string for a root child.</summary>
    public string Parent { get; }

    /// <summary>Gets whether this is an inferred directory that has no physical ZIP entry.</summary>
    public bool IsSynthetic => Entry is null;

    internal static ZipArchiveEntryInfo CreateSyntheticDirectory(string fullName)
    {
        return new ZipArchiveEntryInfo(null, NormalizePath(fullName), true);
    }

    internal static string NormalizePath(string path)
    {
        return path
            .Replace('\\', '/')
            .Split(['/'], StringSplitOptions.RemoveEmptyEntries)
            .JoinWith("/");
    }
}
