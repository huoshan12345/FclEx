using static FclEx.Extensions.FileConflictOptions;

namespace FclEx.Extensions;

public static class FileInfoExtensions
{
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

        if (dest.Exists && FileHelper.AreFilesEqual(file, dest))
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

    public static async Task CopyToAsync(this FileInfo file, FileInfo dest, FileConflictOptions options, int bufferSize = 4096)
    {
        Check.NotNull(file);
        Check.NotNull(dest);

        if (file.FullName == dest.FullName)
            return;

        using var _ = Disposable.Create(() =>
        {
            file.Refresh();
            dest.Refresh();
        });

        if (dest.Exists == false)
        {
            await CopyToAsync(file, dest, bufferSize);
            return;
        }

        var option = options & ~DeleteOnSame; // remove flag

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (option)
        {
            case Cancel: return;
            case Throw: throw new InvalidOperationException("File already exists: " + dest.FullName);
            case Overwrite:
            {
                await CopyToAsync(file, dest, bufferSize);
                return;
            }
            case FileConflictOptions.Rename:
            {
                var newName = FileHelper.GetNextFileName(dest.Name);
                var newDest = new FileInfo(Path.Combine(dest.DirectoryName!, newName));
                await file.CopyToAsync(newDest, options, bufferSize);
                return;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(option), option, null);
        }
    }

    public static Task CopyToAsync(this FileInfo file, DirectoryInfo dir, FileConflictOptions options, int bufferSize = 4096)
    {
        Check.NotNull(file);
        Check.NotNull(dir);

        var dest = new FileInfo(Path.Combine(dir.FullName, file.Name));
        return file.CopyToAsync(dest, options, bufferSize);
    }

#if NETSTANDARD2_0
    private static readonly MethodInfo? _methodMoveTo = typeof(FileInfo).GetMethod(
        name: nameof(FileInfo.MoveTo),
        bindingAttr: BindingFlags.Public | BindingFlags.Instance,
        binder: null,
        types: [typeof(string), typeof(bool)], 
        modifiers: null);

    public static void MoveTo(this FileInfo file, string destFileName, bool overwrite)
    {
        if (_methodMoveTo is {} method)
        {
            method.Invoke(file, [destFileName, overwrite]);
        }
        else if (overwrite == false)
        {
            file.MoveTo(destFileName);
        }
        else
        {
            if (File.Exists(destFileName))
                File.Delete(destFileName);

            file.MoveTo(destFileName);
        }
    }
#endif

    public static void MoveTo(this FileInfo file, FileInfo dest, FileConflictOptions options = Default)
    {
        Check.NotNull(file);
        Check.NotNull(dest);
        Check.EqualTo(file.Exists, true);

        if (file.FullName == dest.FullName)
            return;

        using var _ = Disposable.Create(() =>
        {
            file.Refresh();
            dest.Refresh();
        });

        if (dest.Exists == false)
        {
            file.MoveTo(dest.FullName, false);
            return;
        }

        if (options.HasFlag(DeleteOnSame))
        {
            if (dest.Exists && FileHelper.AreFilesEqual(file, dest))
            {
                file.Delete();
                return;
            }
        }

        var option = options & ~DeleteOnSame; // remove flag

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (option)
        {
            case Cancel: return;
            case Throw: throw new InvalidOperationException("File already exists: " + dest.FullName);
            case Overwrite:
            {
                file.MoveTo(dest.FullName, true);
                return;
            }
            case FileConflictOptions.Rename:
            {
                var newName = FileHelper.GetNextFileName(dest.Name);
                var newDest = new FileInfo(Path.Combine(dest.DirectoryName!, newName));
                file.MoveTo(newDest, options);
                return;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(option), option, null);
        }
    }

    public static void MoveTo(this FileInfo file, DirectoryInfo dir, FileConflictOptions options = Default)
    {
        Check.NotNull(file);
        Check.NotNull(dir);

        var dest = new FileInfo(Path.Combine(dir.FullName, file.Name));
        file.MoveTo(dest, options);
    }

    public static void MoveTo(this FileInfo file, DirectoryInfo? dir, string name, bool appendExt = false, FileConflictOptions options = Default)
    {
        Check.NotNull(file);
        Check.NotEmpty(name);

        var dirPath = (dir?.FullName ?? file.DirectoryName)
                      ?? throw new InvalidOperationException($"Both {nameof(dir)} and {nameof(file)}.{nameof(file.DirectoryName)} are null.");

        if (appendExt)
        {
            var ext = Path.GetExtension(file.Name);
            name += ext;
        }

        var dest = new FileInfo(Path.Combine(dirPath, name));
        file.MoveTo(dest, options);
    }

    public static void Rename(this FileInfo file, string name, bool appendExt = false, FileConflictOptions options = Default)
    {
        file.MoveTo(null, name, appendExt, options);
    }
}