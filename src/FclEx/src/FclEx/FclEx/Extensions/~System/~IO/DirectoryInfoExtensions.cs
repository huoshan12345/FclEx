using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace FclEx.Extensions;

public static class DirectoryInfoExtensions
{
    public static DirectoryInfo TryCreate(this DirectoryInfo di)
    {
        if (di.Exists == false)
        {
            di.Create();
            di.Refresh();
        }
        return di;
    }

    public static void MoveToRecycleBin(this FileInfo fi)
    {
        if (fi.Exists)
            FileSystem.DeleteFile(fi.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
    }

    public static void MoveToRecycleBin(this DirectoryInfo di)
    {
        if (di.Exists)
            FileSystem.DeleteDirectory(di.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
    }

    public static DirectoryInfo CreateNew(this DirectoryInfo di)
    {
        di.MoveToRecycleBin();
        di.Create();
        return di;
    }

    public static DirectoryInfo Sub(this DirectoryInfo dir, string name)
    {
        return new(Path.Combine(dir.FullName, name));
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