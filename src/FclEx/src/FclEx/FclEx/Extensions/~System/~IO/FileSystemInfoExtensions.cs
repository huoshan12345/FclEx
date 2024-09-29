namespace FclEx.Extensions;

public static class FileSystemInfoExtensions
{
    public static bool IsHidden(this FileSystemInfo info)
    {
        return info.Attributes.HasFlag(FileAttributes.Hidden);
    }
}