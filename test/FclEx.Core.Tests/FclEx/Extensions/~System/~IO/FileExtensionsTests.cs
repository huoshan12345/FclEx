namespace FclEx.Extensions;

public class FileExtensionsTests
{
    [Fact]
    public async Task WriteAllTextAsync_WithDefaultEncoding_WritesUtf8WithoutBom()
    {
        var path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, "text");

            var bytes = await File.ReadAllBytesAsync(path);
            Assert.False(bytes.Take(Encoding.UTF8.GetPreamble().Length).SequenceEqual(Encoding.UTF8.GetPreamble()));
            Assert.Equal("text"u8, bytes);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void AreFilesEqual_ShouldReturnTrue_ForIdenticalFiles()
    {
        var path1 = Path.GetTempFileName();
        var path2 = Path.GetTempFileName();

        try
        {
            File.WriteAllText(path1, "Hello World!");
            File.Copy(path1, path2, overwrite: true);

            var f1 = new FileInfo(path1);
            var f2 = new FileInfo(path2);

            Assert.True(File.AreFilesEqual(f1, f2));
        }
        finally
        {
            File.Delete(path1);
            File.Delete(path2);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(4095)]
    [InlineData(4096)]
    [InlineData(4097)]
    [InlineData(8193)]
    public void AreFilesEqual_ShouldCompareOnlyFileContent(int length)
    {
        var path1 = Path.GetTempFileName();
        var path2 = Path.GetTempFileName();

        try
        {
            var content = Enumerable.Range(0, length).Select(i => (byte)i).ToArray();
            File.WriteAllBytes(path1, content);
            File.WriteAllBytes(path2, content);

            Assert.True(File.AreFilesEqual(new FileInfo(path1), new FileInfo(path2)));
        }
        finally
        {
            File.Delete(path1);
            File.Delete(path2);
        }
    }

    [Fact]
    public void AreFilesEqual_ShouldReturnFalse_WhenContentDiffers()
    {
        var path1 = Path.GetTempFileName();
        var path2 = Path.GetTempFileName();

        try
        {
            File.WriteAllText(path1, "Hello World!");
            File.WriteAllText(path2, "Hello World?");

            var f1 = new FileInfo(path1);
            var f2 = new FileInfo(path2);

            Assert.False(File.AreFilesEqual(f1, f2));
        }
        finally
        {
            File.Delete(path1);
            File.Delete(path2);
        }
    }

    [Fact]
    public void AreFilesEqual_ShouldReturnFalse_WhenLengthsDiffer()
    {
        var path1 = Path.GetTempFileName();
        var path2 = Path.GetTempFileName();

        try
        {
            File.WriteAllText(path1, "short");
            File.WriteAllText(path2, "a bit longer");

            var f1 = new FileInfo(path1);
            var f2 = new FileInfo(path2);

            Assert.False(File.AreFilesEqual(f1, f2));
        }
        finally
        {
            File.Delete(path1);
            File.Delete(path2);
        }
    }

    [Fact]
    public void AreFilesEqual_ShouldThrow_WhenFileDoesNotExist()
    {
        var missingFile = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        var tempFile = new FileInfo(Path.GetTempFileName());

        Assert.ThrowsAny<Exception>(() => File.AreFilesEqual(missingFile, tempFile));

        File.Delete(tempFile.FullName);
    }
}
