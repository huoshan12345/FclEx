using System.IO;

namespace FclEx.Abp;

public static class DirectoryInfoExtensions
{
    public static DirectoryInfo EnsureExists(this DirectoryInfo di)
    {
        if (!di.Exists)
            di.Create();
        return di;
    }
}