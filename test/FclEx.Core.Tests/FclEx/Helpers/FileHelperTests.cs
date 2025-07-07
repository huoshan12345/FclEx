namespace FclEx.Helpers;

public class FileHelperTests
{
    [Theory]
    [InlineData(".txt", "_1.txt")]
    [InlineData("x", "x_1")]
    [InlineData("x_1", "x_2")]
    [InlineData("x.txt", "x_1.txt")]
    [InlineData("x.txt.txt", "x.txt_1.txt")]
    [InlineData("x_.txt", "x_1.txt")]
    [InlineData("x_2.txt.txt", "x_2.txt_1.txt")]
    [InlineData("x._1.txt", "x._2.txt")]
    [InlineData("x_1.txt", "x_2.txt")]
    public void GetNextFileName_Test(string fileName, string expected)
    {
        var newName = FileHelper.GetNextFileName(fileName);
        Assert.Equal(expected, newName);
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

            Assert.True(FileHelper.AreFilesEqual(f1, f2));
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
            File.WriteAllText(path2, "Goodbye World!");

            var f1 = new FileInfo(path1);
            var f2 = new FileInfo(path2);

            Assert.False(FileHelper.AreFilesEqual(f1, f2));
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

            Assert.False(FileHelper.AreFilesEqual(f1, f2));
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

        Assert.ThrowsAny<Exception>(() => FileHelper.AreFilesEqual(missingFile, tempFile));

        File.Delete(tempFile.FullName);
    }
}
