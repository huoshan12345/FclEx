namespace Xunit;

public partial class AssertExTests
{
    [Fact]
    public void FileExists_WhenFileExists_ShouldNotThrow()
    {
        var path = Path.GetTempFileName();
        try
        {
            Assert.FileExists(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void FileExists_WhenFileDoesNotExist_ShouldThrow()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var exception = Assert.ThrowsAny<XunitException>(() => Assert.FileExists(path));

        Assert.Contains(path, exception.Message);
    }

    [Fact]
    public void DirectoryExists_WhenDirectoryExists_ShouldNotThrow()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        try
        {
            Assert.DirectoryExists(path);
        }
        finally
        {
            Directory.Delete(path);
        }
    }

    [Fact]
    public void DirectoryExists_WhenDirectoryDoesNotExist_ShouldThrow()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var exception = Assert.ThrowsAny<XunitException>(() => Assert.DirectoryExists(path));

        Assert.Contains(path, exception.Message);
    }

    [Fact]
    public void Exists_WhenFileSystemInfoExists_ShouldNotThrow()
    {
        var path = Path.GetTempFileName();
        try
        {
            Assert.Exists(new FileInfo(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Exists_WhenFileSystemInfoDoesNotExist_ShouldThrow()
    {
        var file = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

        var exception = Assert.ThrowsAny<XunitException>(() => Assert.Exists(file));

        Assert.Contains(file.FullName, exception.Message);
    }

    [Fact]
    public void Exists_WhenFileSystemInfoIsNull_ShouldThrowArgumentNullException()
    {
        FileSystemInfo? fileSystemInfo = null;

        var exception = Assert.Throws<ArgumentNullException>(() => Assert.Exists(fileSystemInfo!));

        Assert.Equal("fileSystemInfo", exception.ParamName);
    }
}
