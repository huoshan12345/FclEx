namespace FclEx.Extensions;

public static class ZipArchiveEntryExtensions
{
    public static async Task ExtractToFileAsync(this ZipArchiveEntry source, string destPath, bool overwrite, int bufferSize = 4 * 1024, CancellationToken token = default)
    {
        Check.NotNull(source);
        Check.NotEmpty(destPath);

        var mode = overwrite ? FileMode.Create : FileMode.CreateNew;

#if NET6_0_OR_GREATER
        await
#endif
        using (var destination = new FileStream(destPath, mode, FileAccess.Write, FileShare.None, bufferSize, false))
        {
#if NET6_0_OR_GREATER
            await
#endif
            using var stream = source.Open();
            await stream.CopyToAsync(destination, bufferSize, token);
        }
        File.SetLastWriteTime(destPath, source.LastWriteTime.DateTime);
    }

    public static Task ExtractToDirAsync(this ZipArchiveEntry entry, string dir, bool ignoreEntryDir, bool overwrite, int bufferSize = 4 * 1024, CancellationToken token = default)
    {
        Check.NotNull(entry);
        Check.NotEmpty(dir);

        var destinationDirectory = Path.GetFullPath(dir)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var entryPath = ignoreEntryDir ? entry.Name : entry.FullName;
        var path = Path.GetFullPath(Path.Combine(destinationDirectory, entryPath));
        var comparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!path.StartsWith(destinationDirectory, comparison))
            throw new InvalidDataException($"The ZIP entry '{entry.FullName}' would be extracted outside the destination directory.");

        var fi = new FileInfo(path);
        fi.Directory?.TryCreate();
        return entry.ExtractToFileAsync(fi.FullName, overwrite, bufferSize, token);
    }

    public static bool IsDirectory(this ZipArchiveEntry entry)
    {
        var last = entry.FullName.LastOrDefault();
        return last is '/' or '\\' && entry.Name == "";
    }

    public static bool IsFile(this ZipArchiveEntry entry)
    {
        return !entry.IsDirectory();
    }

    /// <summary>Builds a hierarchy for all files and directories in an archive.</summary>
    /// <remarks>
    /// Paths are normalized to <c>/</c>-separated full paths. Missing directory entries are represented by synthetic
    /// <see cref="ZipArchiveEntryInfo"/> nodes whose <see cref="ZipArchiveEntryInfo.Entry"/> is <see langword="null"/>.
    /// Directories precede files and each group is ordered by name.
    /// </remarks>
    public static TreeNode<ZipArchiveEntryInfo> BuildTree(this ZipArchive archive)
    {
        Check.NotNull(archive);

        var root = new TreeNode<ZipArchiveEntryInfo>(default);
        var entries = archive.Entries
            .Where(entry => ZipArchiveEntryInfo.NormalizePath(entry.FullName).Length != 0)
            .Select(entry => new ZipArchiveEntryInfo(entry))
            .ToArray();
        var explicitDirectories = entries
            .Where(entry => entry.IsDirectory)
            .GroupBy(entry => entry.FullName, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var directoryNodes = new Dictionary<string, TreeNode<ZipArchiveEntryInfo>>(StringComparer.Ordinal)
        {
            [string.Empty] = root,
        };

        foreach (var entry in entries)
        {
            if (entry.IsDirectory)
                EnsureDirectory(entry.FullName);
            else
                EnsureDirectory(entry.Parent).AddChild(entry);
        }

        var nodes = new Queue<TreeNode<ZipArchiveEntryInfo>>();
        nodes.Enqueue(root);
        while (nodes.Count != 0)
        {
            var node = nodes.Dequeue();
            node.Children.Sort((left, right) =>
            {
                var typeComparison = right.Value.IsDirectory.CompareTo(left.Value.IsDirectory);
                return typeComparison != 0
                    ? typeComparison
                    : StringComparer.Ordinal.Compare(left.Value.Name, right.Value.Name);
            });
            foreach (var child in node.Children.Where(child => child.Value.IsDirectory))
                nodes.Enqueue(child);
        }

        return root;

        TreeNode<ZipArchiveEntryInfo> EnsureDirectory(string path)
        {
            if (directoryNodes.TryGetValue(path, out var existing))
                return existing;

            var separatorIndex = path.LastIndexOf('/');
            var parentPath = separatorIndex < 0 ? string.Empty : path[..separatorIndex];
            var parent = EnsureDirectory(parentPath);
            var info = explicitDirectories.TryGetValue(path, out var explicitDirectory)
                ? explicitDirectory
                : ZipArchiveEntryInfo.CreateSyntheticDirectory(path);
            var created = parent.AddChild(info);
            directoryNodes.Add(path, created);
            return created;
        }
    }
}
