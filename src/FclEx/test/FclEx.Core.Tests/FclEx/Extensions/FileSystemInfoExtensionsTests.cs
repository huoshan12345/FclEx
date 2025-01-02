namespace FclEx.Extensions;

public class FileSystemInfoExtensionsTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("C:", "C:")]
    [InlineData(@"C:\", "C:")]
    [InlineData(@"C:\foo\bar", "C:")]
    [InlineData(@"\\network-machine\foo\bar", @"\\network-machine")]
    [InlineData(@"\\network-machine", @"\\network-machine")]
    [InlineData(@"\\network-machine\", @"\\network-machine")]
    public void GetRoot_Directory_Test(string path, string expected)
    {
        Assert.Equal(expected, new DirectoryInfo(path).GetRoot());
    }

    [Theory]
    [InlineData(@"C:\foo\bar\test.txt", "C:")]
    [InlineData(@"\\network-machine\foo\bar\test.txt", @"\\network-machine")]
    [InlineData(@"\\network-machine\test.txt", @"\\network-machine")]
    public void GetRoot_File_Test(string path, string expected)
    {
        Assert.Equal(expected, new FileInfo(path).GetRoot());
    }

    [Theory]
    [InlineData("C:", @"\")]
    [InlineData(@"C:\", @"\")]
    [InlineData(@"C:\foo\bar", @"\foo\bar\")]
    [InlineData(@"C:\foo\bar\", @"\foo\bar\")]
    [InlineData(@"\\network-machine\foo\bar", @"\foo\bar\")]
    [InlineData(@"\\network-machine\foo\bar\", @"\foo\bar\")]
    [InlineData(@"\\network-machine\", @"\")]
    [InlineData(@"\\network-machine", @"\")]
    public void GetFullPathWithoutRoot_Directory_Test(string path, string expected)
    {
        Assert.Equal(expected, new DirectoryInfo(path).GetFullPathWithoutRoot());
    }

    [Theory]
    [InlineData(@"C:\test.txt", @"\test.txt")]
    [InlineData(@"C:\foo\bar\test.txt", @"\foo\bar\test.txt")]
    [InlineData(@"\\network-machine\test.txt", @"\test.txt")]
    [InlineData(@"\\network-machine\foo\bar\test.txt", @"\foo\bar\test.txt")]
    public void GetFullPathWithoutRoot_File_Test(string path, string result)
    {
        output.WriteLine(Path.GetPathRoot(path));

        Assert.Equal(result, new FileInfo(path).GetFullPathWithoutRoot());
    }

    [Theory]
    [InlineData("C:", 0)]
    [InlineData(@"C:\", 0)]
    [InlineData(@"C:\foo", 1)]
    [InlineData(@"C:\foo\", 1)]
    [InlineData(@"C:\foo\bar", 2)]
    [InlineData(@"\\network-machine\foo\bar", 2)]
    [InlineData(@"\\network-machine", 0)]
    [InlineData(@"\\network-machine\", 0)]
    public void GetLevel_Directory_Test(string path, int expected)
    {
        Assert.Equal(expected, new DirectoryInfo(path).GetLevel());
    }


    [Theory]
    [InlineData(@"C:\test.txt", 1)]
    [InlineData(@"C:\foo\bar\test.txt", 3)]
    [InlineData(@"\\network-machine\test.txt", 1)]
    [InlineData(@"\\network-machine\foo\bar\test.txt", 3)]
    public void GetLevel_File_Test(string path, int expected)
    {
        Assert.Equal(expected, new FileInfo(path).GetLevel());
    }

    [Theory]
    [InlineData("C:", "")]
    [InlineData(@"C:\", "")]
    [InlineData(@"C:\foo\bar", "foo")]
    [InlineData(@"C:\foo\bar\", "foo")]
    [InlineData(@"\\network-machine\foo\bar", "foo")]
    [InlineData(@"\\network-machine\foo\bar\", "foo")]
    [InlineData(@"\\network-machine\", "")]
    [InlineData(@"\\network-machine", "")]
    public void GetFirstDir_Directory_Test(string path, string expected)
    {
        Assert.Equal(expected, new DirectoryInfo(path).GetFirstDir());
    }

    [Theory]
    [InlineData(@"C:\test.txt", "")]
    [InlineData(@"C:\foo\bar\test.txt", "foo")]
    [InlineData(@"\\network-machine\test.txt", "")]
    [InlineData(@"\\network-machine\foo\bar\test.txt", "foo")]
    public void GetFirstDir_File_Test(string path, string expected)
    {
        output.WriteLine(Path.GetPathRoot(path));
        Assert.Equal(expected, new FileInfo(path).GetFirstDir());
    }
}