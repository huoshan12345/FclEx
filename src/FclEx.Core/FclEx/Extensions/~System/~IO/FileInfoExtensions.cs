using static FclEx.Extensions.FileConflictOptions;

namespace FclEx.Extensions;

public static class FileInfoExtensions
{
    private const FileConflictOptions ResolutionStrategies = Cancel | ThrowOnConflict | Overwrite | AutoRename;

    internal const int DefaultBufferSize = 256 * 1024;

    /// <summary>
    /// Gets the base name and extension of the specified file.
    /// </summary>
    /// <param name="file">The file to extract the name and extension from.</param>
    /// <returns>
    /// A tuple where <c>Name</c> is the file name without extension,
    /// and <c>Ext</c> is the extension (including the leading dot).
    /// </returns>
    public static (string Name, string Ext) GetNameAndExtension(this FileInfo file)
    {
        return PathHelper.GetNameAndExtension(file.Name);
    }

    /// <summary>
    /// Asynchronously copies the file to the specified destination.
    /// Skips the operation if the source and destination are the same file
    /// or if the destination already exists with identical content.
    /// </summary>
    /// <param name="file">The source file.</param>
    /// <param name="dest">The destination file.</param>
    /// <param name="bufferSize">The buffer size in bytes to use during the copy. Default is 4 KB.</param>
    /// <param name="token"></param>
    public static async Task<FileInfo> CopyToAsync(this FileInfo file, FileInfo dest, int bufferSize = 4 * 1024, CancellationToken token = default)
    {
        Check.NotNull(file);
        Check.NotNull(dest);
        Check.EqualTo(file.Exists, true);

        if (file.FullName == dest.FullName
            || dest.Exists && FileHelper.AreFilesEqual(file, dest))
            return dest;

        using var _ = Disposable.Create(dest.Refresh);

#if NET6_0_OR_GREATER
        await
#endif
        using Stream source = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
#if NET6_0_OR_GREATER
        await
#endif
        using Stream destination = File.Create(dest.FullName, bufferSize);
        await source.CopyToAsync(destination, bufferSize, token);

        return dest;
    }

    /// <summary>
    /// Asynchronously copies the file into the specified directory.
    /// </summary>
    /// <param name="file">The source file.</param>
    /// <param name="dir">The destination directory.</param>
    /// <param name="bufferSize">The buffer size in bytes to use during the copy. Default is 4 KB.</param>
    /// <param name="token"></param>
    public static Task<FileInfo> CopyToAsync(this FileInfo file, DirectoryInfo dir, int bufferSize = 4 * 1024, CancellationToken token = default)
    {
        Check.NotNull(file);
        Check.NotNull(dir);

        var dest = new FileInfo(Path.Combine(dir.FullName, file.Name));
        return file.CopyToAsync(dest, bufferSize, token);
    }

    /// <summary>
    /// Asynchronously copies the file to the specified destination with conflict resolution options.
    /// </summary>
    /// <param name="file">The source file.</param>
    /// <param name="dest">The destination file.</param>
    /// <param name="options">Specifies how to handle conflicts when the destination already exists.</param>
    /// <param name="bufferSize">The buffer size in bytes to use during the copy. Default is 4 KB.</param>
    /// <param name="token"></param>
    public static async Task<FileInfo> CopyToAsync(this FileInfo file, FileInfo dest, FileConflictOptions options, int bufferSize = 4 * 1024, CancellationToken token = default)
    {
        Check.NotNull(file);
        Check.NotNull(dest);

        if (file.FullName == dest.FullName)
            return dest;

        using var _ = Disposable.Create(() =>
        {
            file.Refresh();
            dest.Refresh();
        });

        if (dest.Exists == false)
        {
            await file.CopyToAsync(dest, bufferSize, token);
            return dest;
        }

        if (options.HasFlag(IgnoreConflictIfDuplicate))
        {
            if (dest.Exists && FileHelper.AreFilesEqual(file, dest))
            {
                return dest;
            }
        }

        // switch on the pure resolution strategy
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (options & ResolutionStrategies)
        {
            case Cancel: break;
            case ThrowOnConflict: throw new InvalidOperationException("File already exists: " + dest.FullName);
            case Overwrite:
            {
                await file.CopyToAsync(dest, bufferSize, token);
                break;
            }
            case AutoRename:
            {
                var newName = FileHelper.GetNextFileName(dest.Name);
                var newDest = new FileInfo(Path.Combine(dest.DirectoryName!, newName));
                await file.CopyToAsync(newDest, options, bufferSize, token);
                break;
            }
        }

        return dest;
    }

    /// <summary>
    /// Asynchronously copies the file into the specified directory with conflict resolution options.
    /// </summary>
    /// <param name="file">The source file.</param>
    /// <param name="dir">The destination directory.</param>
    /// <param name="options">Specifies how to handle conflicts when the file already exists in the directory.</param>
    /// <param name="bufferSize">The buffer size in bytes to use during the copy. Default is 4 KB.</param>
    /// <param name="token"></param>
    public static Task<FileInfo> CopyToAsync(this FileInfo file, DirectoryInfo dir, FileConflictOptions options, int bufferSize = 4096, CancellationToken token = default)
    {
        Check.NotNull(file);
        Check.NotNull(dir);

        var dest = new FileInfo(Path.Combine(dir.FullName, file.Name));
        return file.CopyToAsync(dest, options, bufferSize, token);
    }

#if NETSTANDARD2_0
    private static readonly MethodInfo? _methodMoveTo = typeof(FileInfo).GetMethod(
        name: nameof(FileInfo.MoveTo),
        bindingAttr: BindingFlags.Public | BindingFlags.Instance,
        binder: null,
        types: [typeof(string), typeof(bool)],
        modifiers: null);

    /// <summary>
    /// Moves the file to a new location, optionally overwriting the target file.
    /// This overload exists for .NET Standard 2.0 compatibility.
    /// </summary>
    /// <param name="file">The source file.</param>
    /// <param name="destFileName">The full path of the destination file.</param>
    /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
    public static void MoveTo(this FileInfo file, string destFileName, bool overwrite)
    {
        if (_methodMoveTo is { } method)
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

    /// <summary>
    /// Moves the file to the specified destination with conflict resolution options.
    /// </summary>
    /// <param name="file">The source file.</param>
    /// <param name="dest">The destination file.</param>
    /// <param name="options">Specifies how to handle conflicts. Default is <see cref="FileConflictOptions.Default"/>.</param>
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

        if (options.HasFlag(IgnoreConflictIfDuplicate))
        {
            if (dest.Exists && FileHelper.AreFilesEqual(file, dest))
            {
                file.Delete();
                return;
            }
        }

        // switch on the pure resolution strategy
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (options & ResolutionStrategies)
        {
            case Cancel: return;
            case ThrowOnConflict: throw new InvalidOperationException("File already exists: " + dest.FullName);
            case Overwrite:
            {
                file.MoveTo(dest.FullName, true);
                return;
            }
            case AutoRename:
            {
                var newName = FileHelper.GetNextFileName(dest.Name);
                var newDest = new FileInfo(Path.Combine(dest.DirectoryName!, newName));
                file.MoveTo(newDest, options);
                return;
            }
        }
    }

    /// <summary>
    /// Moves the file into the specified directory with conflict resolution options.
    /// </summary>
    /// <param name="file">The source file.</param>
    /// <param name="dir">The destination directory.</param>
    /// <param name="options">Specifies how to handle conflicts. Default is <see cref="FileConflictOptions.Default"/>.</param>
    public static void MoveTo(this FileInfo file, DirectoryInfo dir, FileConflictOptions options = Default)
    {
        Check.NotNull(file);
        Check.NotNull(dir);

        var dest = new FileInfo(Path.Combine(dir.FullName, file.Name));
        file.MoveTo(dest, options);
    }

    /// <summary>
    /// Moves the file to a new directory and/or renames it.
    /// </summary>
    /// <param name="file">The source file.</param>
    /// <param name="dir">The destination directory, or <c>null</c> to use the file’s current directory.</param>
    /// <param name="name">The new file name.</param>
    /// <param name="appendExt">Whether to append the original extension to the new name.</param>
    /// <param name="options">Specifies how to handle conflicts. Default is <see cref="FileConflictOptions.Default"/>.</param>
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

    /// <summary>
    /// Renames the file within its current directory.
    /// </summary>
    /// <param name="file">The source file.</param>
    /// <param name="name">The new file name.</param>
    /// <param name="appendExt">Whether to append the original extension to the new name.</param>
    /// <param name="options">Specifies how to handle conflicts. Default is <see cref="FileConflictOptions.Default"/>.</param>
    public static void Rename(this FileInfo file, string name, bool appendExt = false, FileConflictOptions options = Default)
    {
        file.MoveTo(null, name, appendExt, options);
    }

    public static Task WriteAllTextAsync(this FileInfo file, string content, Encoding? encoding = null, CancellationToken token = default)
    {
        Check.NotNull(file);
        Check.NotNull(content);

        return File.WriteAllTextAsync(file.FullName, content, encoding ?? Encoding.UTF8, token);
    }

    public static FileInfo CopyTo(this FileInfo fileInfo, FileInfo destFileInfo, bool overwrite = false)
    {
        Check.NotNull(fileInfo);
        Check.NotNull(destFileInfo);

        return fileInfo.FullName == destFileInfo.FullName
            ? destFileInfo
            : fileInfo.CopyTo(destFileInfo.FullName, overwrite);
    }
}