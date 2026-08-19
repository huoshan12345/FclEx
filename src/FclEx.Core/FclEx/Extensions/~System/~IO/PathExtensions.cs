namespace FclEx.Extensions;

public static class PathExtensions
{
    private static readonly Regex FileNumberSuffix = new(@"_(\d*)$", RegexOptions.Compiled);

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

        /// <summary>
        /// Gets the base name and extension of the specified file.
        /// </summary>
        /// <param name="fileName">The file name to extract the name and extension from.</param>
        /// <returns>
        /// A tuple where <c>Name</c> is the file name without extension,
        /// and <c>Ext</c> is the extension (including the leading dot).
        /// </returns>
        public static (string Name, string Ext) GetNameAndExtension(string fileName)
        {
            return fileName.Partition(".", SeparatorLocationOption.Right, true);
        }


        /// <summary>
        /// Generates a new file name by incrementing the numeric suffix.
        /// If the file name already ends with "_&lt;number&gt;", that number is incremented.
        /// Otherwise, "_1" is appended before the extension.
        /// </summary>
        /// <param name="fileName">The original file name (with or without extension).</param>
        /// <returns>A new file name with an incremented or added numeric suffix.</returns>
        public static string GetNextFileName(string fileName)
        {
            var (name, ext) = Path.GetNameAndExtension(fileName);
            var newName = FileNumberSuffix.Replace(name, 1, v => v.ToInt() + 1, s => s + "_1");
            return newName + ext;
        }
    }
}
