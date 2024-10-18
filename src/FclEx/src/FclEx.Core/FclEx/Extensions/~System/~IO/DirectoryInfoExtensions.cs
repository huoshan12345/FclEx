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
        if (dir.Exists == false)
        {
            dir.Delete(recursive);
            dir.Refresh();
        }
        return dir;
    }

    public static DirectoryInfo CreateNew(this DirectoryInfo dir)
    {
        dir.Delete(true);
        dir.Create();
        return dir;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DirectoryInfo Sub(this DirectoryInfo dir, string name)
    {
        return new(Path.Combine(dir.FullName, name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FileInfo File(this DirectoryInfo dir, string name)
    {
        return new FileInfo(Path.Combine(dir.FullName, name));
    }

    private static readonly ConcurrentDictionary<string, string> _pathWithSepCache = new();
    public static bool IsSubOf(this DirectoryInfo sub, DirectoryInfo parent)
    {
        var path = _pathWithSepCache.GetOrAdd(parent.FullName, m => m + Path.DirectorySeparatorChar);
        return sub.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsEmpty(this DirectoryInfo dir)
    {
        return dir.EnumerateFileSystemInfos().Any() == false;
    }
}