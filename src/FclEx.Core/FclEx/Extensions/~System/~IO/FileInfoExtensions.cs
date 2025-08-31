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

    public static async Task CopyToAsync(this FileInfo file, FileInfo dest, int bufferSize = 4 * 1024)
    {
        Check.NotNull(file);
        Check.NotNull(dest);
        Check.EqualTo(file.Exists, true);

        if (file.FullName == dest.FullName)
            return;

        if (dest.Exists && FileHelper.AreSame(file, dest))
            return;

        using var _ = Disposable.Create(dest.Refresh);

#if NET6_0_OR_GREATER
        await
#endif
        using Stream source = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
#if NET6_0_OR_GREATER
        await
#endif
        using Stream destination = File.Create(dest.FullName, bufferSize);
        await source.CopyToAsync(destination);
    }

    public static Task CopyToAsync(this FileInfo file, DirectoryInfo dir, int bufferSize = 4 * 1024)
    {
        Check.NotNull(file);
        Check.NotNull(dir);

        var dest = new FileInfo(Path.Combine(dir.FullName, file.Name));
        return file.CopyToAsync(dest, bufferSize);
    }

}