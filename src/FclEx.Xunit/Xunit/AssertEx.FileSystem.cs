namespace Xunit;

partial class AssertEx
{
    extension(Assert)
    {
        public static void FileExists(string path)
        {
            Assert.True(File.Exists(path), () => $"File does not exist: {path}");
        }

        public static void DirectoryExists(string path)
        {
            Assert.True(Directory.Exists(path), () => $"Directory does not exist: {path}");
        }

        public static void Exists(FileSystemInfo fileSystemInfo)
        {
            Assert.True(fileSystemInfo.Exists, () => $"Path does not exist: {fileSystemInfo.FullName}");
        }
    }
}
