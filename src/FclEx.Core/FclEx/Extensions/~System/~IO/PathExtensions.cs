namespace FclEx.Extensions;

public static class PathExtensions
{
    extension(Path)
    {
        public static DirectoryInfo ToDirectoryInfo(params string[] paths)
        {
            var path = Path.Combine(paths);
            return new DirectoryInfo(path);
        }

        public static FileInfo ToFileInfo(params string[] paths)
        {
            var path = Path.Combine(paths);
            return new FileInfo(path);
        }
    }
}
