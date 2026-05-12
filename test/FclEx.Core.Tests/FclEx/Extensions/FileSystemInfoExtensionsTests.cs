namespace FclEx.Extensions;

public class FileSystemInfoExtensionsTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Theory]
    // Windows Local Paths
    [InlineData(@"C:\", @"C:\", true)]
    [InlineData(@"C:\foo\bar.txt", @"C:\", false)]
    // Windows UNC Paths
    [InlineData(@"\\server\share\", @"\\server\share\", true)]
    [InlineData(@"\\server\share\file.txt", @"\\server\share\", false)]
    // Linux Paths
    [InlineData("/", "/", true)]
    [InlineData("/home/user", "/", false)]
    public void GetRootPath_ShouldReturnCorrectRoot(string path, string expectedRoot, bool isRoot)
    {
        // Skip Windows tests on Linux and vice versa
        if (IsWindows != path.Contains('\\')) 
            return;

        // Special handling for NFX: new DirectoryInfo(@"\\server") throws ArgumentException
        // Our GetRootPath should handle the string logic even if the object construction is tricky
        var info = CreateInfo(path);

        var actualRoot = info.GetRootPath();
        Assert.Equal(expectedRoot, actualRoot);
        Assert.Equal(isRoot, info.IsRoot());
    }

    [Theory]
    [InlineData(@"C:\foo\bar\", @"\foo\bar\")]
    [InlineData(@"C:\foo\file.txt", @"\foo\file.txt")]
    [InlineData(@"\\server\share\sub\file.txt", @"\sub\file.txt")]
    [InlineData("/", @"\")] // Linux root maps to our defined separator
    public void GetRelativeRootPath_ShouldReturnFormattedPath(string fullPath, string expectedRelative)
    {
        if (IsWindows != fullPath.Contains('\\')) 
            return;

        var info = CreateInfo(fullPath);
        var result = info.GetRelativeRootPath();

        // On Linux, Path.DirectorySeparatorChar is '/', adjust expectation
        var platformExpected = expectedRelative.Replace('\\', Path.DirectorySeparatorChar);
        Assert.Equal(platformExpected, result);
    }

    [Theory]
    [InlineData(@"C:\foo\bar\", "foo")]
    [InlineData(@"C:\temp\test.txt", "temp")]
    [InlineData(@"\\server\share\photos\cat.jpg", "photos")]
    [InlineData(@"C:\", "")]
    public void GetTopLevelDirectoryName_ShouldExtractFirstFolder(string path, string expectedName)
    {
        if (IsWindows != path.Contains('\\')) 
            return;

        var info = CreateInfo(path);
        Assert.Equal(expectedName, info.GetTopLevelDirectoryName());
    }

    [Theory]
    [InlineData(@"C:\", 0)]
    [InlineData(@"C:\foo\", 1)]
    [InlineData(@"C:\foo\bar.txt", 2)]
    [InlineData(@"\\server\share\a\b\c.txt", 3)]
    public void GetDepth_ShouldReturnCorrectInteger(string path, int expectedDepth)
    {
        if (IsWindows != path.Contains('\\'))
            return;

        var info = CreateInfo(path);
        Assert.Equal(expectedDepth, info.GetDepth());
    }

    /// <summary>
    /// Helper to create FileInfo or DirectoryInfo based on path string.
    /// Handles NFX vs NET5+ UNC constraints.
    /// </summary>
    private static FileSystemInfo CreateInfo(string path)
    {
        // On .NET Framework, normalizing a partial UNC like "\\server" throws.
        // We wrap it in a try-catch or handle it via conditional compilation.
        try
        {
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()) || !path.Contains("."))
                return new DirectoryInfo(path);
            return new FileInfo(path);
        }
        catch (ArgumentException) when (!IsNet5OrNewer())
        {
            // If it's NFX and it's a "broken" UNC, we might need a dummy or a more manual test
            // Since we are testing EXTENSIONS, we can sometimes mock the FileSystemInfo if needed
            throw;
        }
    }

    private static bool IsNet5OrNewer()
    {
#if NET5_0_OR_GREATER
        return true;
#else
        return false;
#endif
    }
}