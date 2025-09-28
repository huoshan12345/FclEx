namespace FclEx.Extensions;

public class FileInfoExtensionsTests
{
    [Fact]
    public async Task CopyToAsync_CopiesFileContents()
    {
        var srcPath = Path.GetTempFileName();
        var destPath = Path.GetTempFileName();
        await File.WriteAllTextAsync(srcPath, "Hello World");
        File.Delete(destPath); // ensure dest doesn't exist

        var src = new FileInfo(srcPath);
        var dest = new FileInfo(destPath);

        await src.CopyToAsync(dest);

        Assert.True(dest.Exists);
        Assert.Equal("Hello World", await File.ReadAllTextAsync(dest.FullName));

        Cleanup(src, dest);
    }

    [Fact]
    public async Task CopyToAsync_WhenSourceAndDestinationAreSamePath_NoCopyPerformed()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "Same File");
        var file = new FileInfo(path);

        await file.CopyToAsync(file);

        Assert.True(File.Exists(path));
        Assert.Equal("Same File", await File.ReadAllTextAsync(path));

        Cleanup(file);
    }

    [Fact]
    public async Task CopyToAsync_WhenFilesAreFilesEqual_NoCopyPerformed()
    {
        var srcPath = Path.GetTempFileName();
        await File.WriteAllTextAsync(srcPath, "Test Content");

        var src = new FileInfo(srcPath);
        var dest = new FileInfo(srcPath); // simulate same file
        await src.CopyToAsync(dest);

        Assert.True(dest.Exists);
        Assert.Equal("Test Content", await File.ReadAllTextAsync(dest.FullName));

        Cleanup(src, dest);
    }

    [Fact]
    public async Task CopyToAsync_OverwritesExistingFile()
    {
        var srcPath = Path.GetTempFileName();
        var destPath = Path.GetTempFileName();
        await File.WriteAllTextAsync(srcPath, "Source Content");
        await File.WriteAllTextAsync(destPath, "Old Content");

        var src = new FileInfo(srcPath);
        var dest = new FileInfo(destPath);
        await src.CopyToAsync(dest);

        Assert.Equal("Source Content", await File.ReadAllTextAsync(dest.FullName));

        Cleanup(src, dest);
    }

    [Fact]
    public async Task CopyToAsync_Throws_WhenSourceFileDoesNotExist()
    {
        var src = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        var dest = new FileInfo(Path.GetTempFileName());

        await Assert.ThrowsAsync<ArgumentException>(() => src.CopyToAsync(dest));
    }

    [Fact]
    public async Task CopyToAsync_Throws_WhenSourceIsNull()
    {
        FileInfo src = null!;
        var dest = new FileInfo(Path.GetTempFileName());

        await Assert.ThrowsAsync<ArgumentNullException>(() => src.CopyToAsync(dest));
    }

    [Fact]
    public async Task CopyToAsync_Throws_WhenDestinationIsNull()
    {
        var srcPath = Path.GetTempFileName();
        await File.WriteAllTextAsync(srcPath, "Some data");
        var src = new FileInfo(srcPath);

        FileInfo dest = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() => src.CopyToAsync(dest));
    }

    [Fact]
    public async Task CopyToAsync_RespectsBufferSize()
    {
        var srcPath = Path.GetTempFileName();
        var destPath = Path.GetTempFileName();
        await File.WriteAllBytesAsync(srcPath, new byte[100_000]); // large file
        File.Delete(destPath);

        var src = new FileInfo(srcPath);
        var dest = new FileInfo(destPath);

        await src.CopyToAsync(dest, bufferSize: 128);

        Assert.Equal(100_000, new FileInfo(destPath).Length);

        Cleanup(src, dest);
    }

    private static FileInfo CreateTempFile(string? content = null, string? dir = null, string? name = null)
    {
        dir ??= Path.GetTempPath();
        name ??= Path.GetRandomFileName();
        string path = Path.Combine(dir, name);
        File.WriteAllText(path, content ?? "test");
        return new FileInfo(path);
    }

    private static DirectoryInfo CreateTempDir()
    {
        string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        return Directory.CreateDirectory(path);
    }

    // Cleanup helper
    private static void Cleanup(params FileSystemInfo[] infos)
    {
        foreach (var info in infos)
        {
            try
            {
                if (info is FileInfo { Exists: true } f)
                    f.Delete();

                if (info is DirectoryInfo { Exists: true } d)
                    d.Delete(true);
            }
            catch { /* ignore */ }
        }
    }

    [Fact]
    public void GetNameAndExtension_ShouldReturnCorrectParts()
    {
        var file = new FileInfo("example.txt");
        var (name, ext) = file.GetNameAndExtension();
        Assert.Equal("example", name);
        Assert.Equal(".txt", ext);
    }

    [Fact]
    public async Task CopyToAsync_ShouldCopyFile()
    {
        var source = CreateTempFile("hello");
        var dest = new FileInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

        await source.CopyToAsync(dest);

        Assert.True(dest.Exists);
        Assert.Equal("hello", await File.ReadAllTextAsync(dest.FullName));

        Cleanup(source, dest);
    }

    [Fact]
    public async Task CopyToAsync_SameFile_ShouldSkip()
    {
        var file = CreateTempFile("hello");
        await file.CopyToAsync(file);
        Assert.True(file.Exists);
        Cleanup(file);
    }

    [Fact]
    public async Task CopyToAsync_WithConflict_Overwrite()
    {
        var source = CreateTempFile("new");
        var dest = CreateTempFile("old");

        await source.CopyToAsync(dest, FileConflictOptions.Overwrite);

        Assert.Equal("new", await File.ReadAllTextAsync(dest.FullName));

        Cleanup(source, dest);
    }

    [Fact]
    public async Task CopyToAsync_WithConflict_Rename()
    {
        var source = CreateTempFile("hello");
        var dir = CreateTempDir();
        var dest = new FileInfo(Path.Combine(dir.FullName, source.Name));
        await File.WriteAllTextAsync(dest.FullName, "existing");

        await source.CopyToAsync(dir, FileConflictOptions.AutoRename);

        // Should not overwrite existing file
        Assert.Equal("existing", await File.ReadAllTextAsync(dest.FullName));

        // Should create new renamed file
        Assert.True(dir.GetFiles().Length == 2);

        Cleanup(source, dest, dir);
    }

    [Fact]
    public void MoveTo_WithConflict_IgnoreDuplicate()
    {
        var dir = CreateTempDir();
        var dest = CreateTempFile("same", dir.FullName, "a.txt");
        var source = CreateTempFile("same", dir.FullName, "b.txt");

        source.MoveTo(dest, FileConflictOptions.IgnoreConflictIfDuplicate);

        Assert.False(source.Exists); // deleted because identical
        Assert.True(dest.Exists);    // kept

        Cleanup(dest, dir);
    }

    [Fact]
    public void MoveTo_WithConflict_ThrowOnConflict()
    {
        var dir = CreateTempDir();
        var dest = CreateTempFile("data1", dir.FullName, "a.txt");
        var source = CreateTempFile("data2", dir.FullName, "b.txt");

        Assert.Throws<InvalidOperationException>(() =>
            source.MoveTo(dest, FileConflictOptions.ThrowOnConflict));

        Cleanup(source, dest, dir);
    }

    [Fact]
    public void Rename_ShouldChangeName()
    {
        var file = CreateTempFile("hello");
        var newName = Path.GetRandomFileName();

        file.Rename(newName);

        Assert.False(File.Exists(Path.Combine(file.DirectoryName!, newName + ".txt"))); // unless appendExt
        Assert.True(file.Exists);

        Cleanup(file);
    }
}