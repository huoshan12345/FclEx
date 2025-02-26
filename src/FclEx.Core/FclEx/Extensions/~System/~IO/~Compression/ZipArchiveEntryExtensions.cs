namespace FclEx.Extensions;

public static class ZipArchiveEntryExtensions
{
    public static async Task ExtractToFileAsync(this ZipArchiveEntry source, string destPath, bool overwrite)
    {
        Check.NotNull(source);
        Check.NotEmpty(destPath);

        var mode = overwrite ? FileMode.Create : FileMode.CreateNew;

#if NET6_0_OR_GREATER
        await
#endif
        using (var destination = new FileStream(destPath, mode, FileAccess.Write, FileShare.None, 4096, false))
        {
#if NET6_0_OR_GREATER
            await
#endif
            using var stream = source.Open();
            await stream.CopyToAsync(destination);
        }
        File.SetLastWriteTime(destPath, source.LastWriteTime.DateTime);
    }

    public static Task ExtractToDirAsync(this ZipArchiveEntry entry, string dir, bool ignoreEntryDir, bool overwrite)
    {
        var path = ignoreEntryDir
            ? Path.Combine(dir, entry.Name)
            : Path.Combine(dir, entry.FullName);

        var fi = new FileInfo(path);
        fi.Directory?.TryCreate();
        return entry.ExtractToFileAsync(fi.FullName, overwrite);
    }

    public static string Name(this ZipArchiveEntryInfo info)
    {
        return info.Segments.Last();
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

    public static TreeNode<ZipArchiveEntryInfo> BuildTree(this ZipArchive archive)
    {
        var entries = archive.Entries
            .Select(m => new ZipArchiveEntryInfo(m))
            .GroupBy(m => m.Segments.Length);

        var root = new TreeNode<ZipArchiveEntryInfo>(default);

        var parents = new[] { (root, ".") }.ToDictionary(m => m.Item2, m => m.Item1);
        foreach (var group in entries)
        {
            var dic = new Dictionary<string, TreeNode<ZipArchiveEntryInfo>>();

            foreach (var info in group
                         .OrderBy(m => m.IsDirectory == false)
                         .ThenBy(m => m.Name()))
            {
                var parent = parents[info.Parent];
                var child = parent.AddChild(info);
                if (info.IsDirectory)
                {
                    dic.Add(info.Name(), child);
                }
            }
            parents = dic;
        }
        return root;
    }
}