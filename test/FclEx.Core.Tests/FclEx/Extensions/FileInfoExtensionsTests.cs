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
    }

    [Fact]
    public async Task CopyToAsync_WhenFilesAreSame_NoCopyPerformed()
    {
        var srcPath = Path.GetTempFileName();
        await File.WriteAllTextAsync(srcPath, "Test Content");

        var src = new FileInfo(srcPath);
        var dest = new FileInfo(srcPath); // simulate same file
        await src.CopyToAsync(dest);

        Assert.True(dest.Exists);
        Assert.Equal("Test Content", await File.ReadAllTextAsync(dest.FullName));
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
    }
}