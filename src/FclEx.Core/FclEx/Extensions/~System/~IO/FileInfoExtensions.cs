namespace FclEx.Extensions;

public static class FileInfoExtensions
{
    internal const int DefaultBufferSize = 256 * 1024;

    /// <summary>Gets the base name and extension of the specified file.</summary>
    public static (string Name, string Ext) GetNameAndExtension(this FileInfo file)
    {
        Check.NotNull(file);
        return PathHelper.GetNameAndExtension(file.Name);
    }

    /// <summary>Asynchronously copies a file using the requested conflict behavior.</summary>
    /// <remarks>
    /// Except for <see cref="FileConflictResolution.Overwrite"/>, destination creation is atomic and never overwrites a
    /// file created concurrently. A partially written newly-created destination is removed if copying fails.
    /// </remarks>
    /// <returns>The actual destination, which can differ from <paramref name="destination"/> when auto-renaming.</returns>
    public static async Task<FileInfo> CopyToAsync(
        this FileInfo source,
        FileInfo destination,
        FileTransferOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(source);
        Check.NotNull(destination);

        source.Refresh();
        Check.EqualTo(source.Exists, true);

        options ??= FileTransferOptions.Default;
        options.ValidateForCopy();

        if (FileSystemInfoEqualityComparer.Auto.Equals(source, destination))
            return destination;

        if (options.ConflictResolution == FileConflictResolution.Overwrite)
        {
            destination.Refresh();
            if (IsIgnoredDuplicate(source, destination, options))
                return destination;

            await CopyContentsAsync(source, destination, options.BufferSize, FileMode.Create, cancellationToken).NoCapture();
            destination.Refresh();
            return destination;
        }

        var candidate = destination;
        while (DestinationExists(candidate))
        {
            if (IsIgnoredDuplicate(source, candidate, options))
                return candidate;

            switch (options.ConflictResolution)
            {
                case FileConflictResolution.Cancel:
                    return candidate;
                case FileConflictResolution.Throw:
                    throw new IOException("The destination file already exists: " + candidate.FullName);
                case FileConflictResolution.AutoRename:
                    candidate = GetNextDestination(candidate);
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        var stagingFile = await CreateStagedCopyAsync(source, candidate, options.BufferSize, cancellationToken).NoCapture();
        var moved = false;
        try
        {
            while (true)
            {
                try
                {
                    File.Move(stagingFile.FullName, candidate.FullName);
                    moved = true;
                    candidate.Refresh();
                    return candidate;
                }
                catch (IOException) when (DestinationExists(candidate))
                {
                    if (IsIgnoredDuplicate(source, candidate, options))
                        return candidate;

                    switch (options.ConflictResolution)
                    {
                        case FileConflictResolution.Cancel:
                            return candidate;
                        case FileConflictResolution.Throw:
                            throw;
                        case FileConflictResolution.AutoRename:
                            candidate = GetNextDestination(candidate);
                            break;
                        default:
                            throw new UnreachableException();
                    }
                }
            }
        }
        finally
        {
            if (moved == false)
                TryDelete(stagingFile.FullName);
        }
    }

    /// <summary>Asynchronously copies a file into a directory using the requested conflict behavior.</summary>
    public static Task<FileInfo> CopyToAsync(
        this FileInfo source,
        DirectoryInfo destinationDirectory,
        FileTransferOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(source);
        Check.NotNull(destinationDirectory);
        return source.CopyToAsync(destinationDirectory.File(source.Name), options, cancellationToken);
    }

    private static async Task<FileInfo> CreateStagedCopyAsync(
        FileInfo source,
        FileInfo destination,
        int bufferSize,
        CancellationToken cancellationToken)
    {
        var directoryName = destination.DirectoryName
                            ?? throw new InvalidOperationException("The destination has no containing directory.");
        var stagingFile = new FileInfo(Path.Combine(directoryName, $".fclex-{Guid.NewGuid():N}.tmp"));
        await CopyContentsAsync(source, stagingFile, bufferSize, FileMode.CreateNew, cancellationToken).NoCapture();
        return stagingFile;
    }

    private static async Task CopyContentsAsync(
        FileInfo source,
        FileInfo destination,
        int bufferSize,
        FileMode destinationMode,
        CancellationToken cancellationToken)
    {
        var removeOnFailure = false;
        try
        {
#if NET6_0_OR_GREATER
            await
#endif
            using var sourceStream = new FileStream(
                source.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
#if NET6_0_OR_GREATER
            await
#endif
            using var destinationStream = new FileStream(
                destination.FullName, destinationMode, FileAccess.Write, FileShare.None, bufferSize, true);
            removeOnFailure = destinationMode == FileMode.CreateNew;
            await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken).NoCapture();
        }
        catch
        {
            if (removeOnFailure)
            {
                try
                {
                    File.Delete(destination.FullName);
                }
                catch
                {
                    // Preserve the exception that interrupted the copy.
                }
            }

            throw;
        }
    }

    private static bool DestinationExists(FileInfo destination)
    {
        destination.Refresh();
        return destination.Exists;
    }

    private static bool IsIgnoredDuplicate(FileInfo source, FileInfo destination, FileTransferOptions options)
    {
        return options.IgnoreConflictIfDuplicate
               && destination.Exists
               && TryAreFilesEqual(source, destination);
    }

    private static bool TryAreFilesEqual(FileInfo source, FileInfo destination)
    {
        try
        {
            return FileHelper.AreFilesEqual(source, destination);
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // Cleanup is best effort and must not hide the operation's result.
        }
    }

    private static FileInfo GetNextDestination(FileInfo destination)
    {
        var directoryName = destination.DirectoryName
                            ?? throw new InvalidOperationException("The destination has no containing directory.");
        return new FileInfo(Path.Combine(directoryName, FileHelper.GetNextFileName(destination.Name)));
    }

#if !NET5_0_OR_GREATER
    private static readonly MethodInfo? _methodMoveTo = typeof(FileInfo).GetMethod(
        name: nameof(FileInfo.MoveTo),
        bindingAttr: BindingFlags.Public | BindingFlags.Instance,
        binder: null,
        types: [typeof(string), typeof(bool)],
        modifiers: null);

    /// <summary>Moves the file to a new location, optionally overwriting the target file.</summary>
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

    /// <summary>Moves a file using the requested conflict behavior.</summary>
    /// <returns>The actual destination, which can differ from <paramref name="destination"/> when auto-renaming.</returns>
    public static FileInfo MoveTo(
        this FileInfo source,
        FileInfo destination,
        FileTransferOptions? options = null)
    {
        Check.NotNull(source);
        Check.NotNull(destination);

        source.Refresh();
        Check.EqualTo(source.Exists, true);

        options ??= FileTransferOptions.Default;
        options.ValidateForMove();

        if (FileSystemInfoEqualityComparer.Auto.Equals(source, destination))
            return destination;

        if (options.ConflictResolution == FileConflictResolution.Overwrite)
        {
            destination.Refresh();
            if (IsIgnoredDuplicate(source, destination, options))
            {
                source.Delete();
                source.Refresh();
                destination.Refresh();
                return destination;
            }

            source.MoveTo(destination.FullName, true);
            destination.Refresh();
            return destination;
        }

        var candidate = destination;
        while (true)
        {
            try
            {
                source.MoveTo(candidate.FullName, false);
                candidate.Refresh();
                return candidate;
            }
            catch (IOException) when (DestinationExists(candidate))
            {
                if (IsIgnoredDuplicate(source, candidate, options))
                {
                    source.Delete();
                    source.Refresh();
                    candidate.Refresh();
                    return candidate;
                }

                switch (options.ConflictResolution)
                {
                    case FileConflictResolution.Cancel:
                        return candidate;
                    case FileConflictResolution.Throw:
                        throw;
                    case FileConflictResolution.AutoRename:
                        candidate = GetNextDestination(candidate);
                        break;
                    default:
                        throw new UnreachableException();
                }
            }
        }
    }

    /// <summary>Moves a file into a directory using the requested conflict behavior.</summary>
    public static FileInfo MoveTo(
        this FileInfo source,
        DirectoryInfo destinationDirectory,
        FileTransferOptions? options = null)
    {
        Check.NotNull(source);
        Check.NotNull(destinationDirectory);
        return source.MoveTo(destinationDirectory.File(source.Name), options);
    }

    /// <summary>Moves a file to a new directory and/or gives it a new name.</summary>
    public static FileInfo MoveTo(
        this FileInfo source,
        DirectoryInfo? destinationDirectory,
        string name,
        bool appendExtension = false,
        FileTransferOptions? options = null)
    {
        Check.NotNull(source);
        Check.NotEmpty(name);

        var directory = destinationDirectory ?? source.Directory
                        ?? throw new InvalidOperationException("The source file has no containing directory.");
        if (appendExtension)
            name += source.Extension;

        return source.MoveTo(directory.File(name), options);
    }

    /// <summary>Renames a file within its current directory.</summary>
    public static FileInfo Rename(
        this FileInfo source,
        string name,
        bool appendExtension = false,
        FileTransferOptions? options = null)
    {
        return source.MoveTo(null, name, appendExtension, options);
    }

    public static FileInfo CopyTo(this FileInfo fileInfo, FileInfo destFileInfo, bool overwrite = false)
    {
        Check.NotNull(fileInfo);
        Check.NotNull(destFileInfo);

        return FileSystemInfoEqualityComparer.Auto.Equals(fileInfo, destFileInfo)
            ? destFileInfo
            : fileInfo.CopyTo(destFileInfo.FullName, overwrite);
    }

    public static Task WriteAllTextAsync(this FileInfo file, string content, Encoding? encoding = null, CancellationToken token = default)
    {
        Check.NotNull(file);
        Check.NotNull(content);
        return File.WriteAllTextAsync(file.FullName, content, encoding ?? Encoding.UTF8, token);
    }

    public static Task<string> ReadAllTextAsync(this FileInfo file, Encoding? encoding = null, CancellationToken token = default)
    {
        Check.NotNull(file);
        return File.ReadAllTextAsync(file.FullName, encoding ?? Encoding.UTF8, token);
    }

    public static string ReadAllText(this FileInfo file, Encoding? encoding = null)
    {
        Check.NotNull(file);
        return File.ReadAllText(file.FullName, encoding ?? Encoding.UTF8);
    }

    public static void WriteAllText(this FileInfo file, string content, Encoding? encoding = null)
    {
        Check.NotNull(file);
        Check.NotNull(content);
        File.WriteAllText(file.FullName, content, encoding ?? Encoding.UTF8);
    }
}
