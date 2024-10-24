namespace FclEx.Helpers;

public static class PathHelper
{
    public static (string Name, string Ext) GetFileNameAndExtension(string fileName)
    {
        return fileName.Partition(".", SeparatorLocationOption.Right, true);
    }
}