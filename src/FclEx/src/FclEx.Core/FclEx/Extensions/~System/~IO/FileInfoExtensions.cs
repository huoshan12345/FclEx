namespace FclEx.Extensions;

public static class FileInfoExtensions
{
    public static FileInfo Rename(this FileInfo file, string name)
    {
        if (file.Name == name)
            return file;

        Check.NotNull(file);
        Check.NotEmpty(name);

        var dir = file.DirectoryName;
        Check.NotNull(dir);

        var newName = Path.Combine(dir, name);
        file.MoveTo(newName);
        return new FileInfo(newName);
    }

    public static (string Name, string Ext) GetFileNameAndExtension(this FileInfo file)
    {
        return PathHelper.GetFileNameAndExtension(file.Name);
    }
}