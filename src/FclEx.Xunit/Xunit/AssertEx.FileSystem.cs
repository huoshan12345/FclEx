namespace Xunit;

partial class AssertEx
{
    extension(Assert)
    {
        /// <summary>
        /// Asserts that a file exists at the specified path.
        /// </summary>
        /// <param name="path">The file path to check.</param>
        public static void FileExists(string path)
        {
            Assert.True(File.Exists(path), () => $"File does not exist: {path}");
        }

        /// <summary>
        /// Asserts that a directory exists at the specified path.
        /// </summary>
        /// <param name="path">The directory path to check.</param>
        public static void DirectoryExists(string path)
        {
            Assert.True(Directory.Exists(path), () => $"Directory does not exist: {path}");
        }

        /// <summary>
        /// Asserts that a file-system entry exists.
        /// </summary>
        /// <param name="fileSystemInfo">The file-system entry to check.</param>
        public static void Exists(FileSystemInfo fileSystemInfo)
        {
            if (fileSystemInfo is null)
                throw new ArgumentNullException(nameof(fileSystemInfo));

            Assert.True(fileSystemInfo.Exists, () => $"Path does not exist: {fileSystemInfo.FullName}");
        }
    }
}
