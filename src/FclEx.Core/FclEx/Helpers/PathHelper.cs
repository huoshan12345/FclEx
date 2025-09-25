namespace FclEx.Helpers;

public static class PathHelper
{
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
}