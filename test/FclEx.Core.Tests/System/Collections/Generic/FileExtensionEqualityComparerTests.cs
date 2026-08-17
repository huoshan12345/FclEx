namespace System.Collections.Generic;

public class FileExtensionEqualityComparerTests
{
    [Theory]
    [InlineData("a.txt", "b.txt", true)]
    [InlineData("a.txt", "b .txt", true)]
    [InlineData("a.txt", ".txt", true)]
    [InlineData("a.txt", "b.t xt", false)]
    [InlineData("a.txt", "btxt", false)]
    [InlineData("a.txt", "b. txt", false)]
    [InlineData("a", "b", true)]
    [InlineData(@"c:\first\a.txt", @"d:\second\b.TXT", true)]
    [InlineData("a.", "b", true)]
    [InlineData(".gitignore", ".GITIGNORE", true)]
    [InlineData(".gitignore", ".editorconfig", false)]
    [InlineData("a.txt", null, false)]
    [InlineData(null, null, true)]
    public void Equals_Test(string? x, string? y, bool equal)
    {
        Assert.Equal(equal, FileExtensionEqualityComparer.Instance.Equals(x, y));
    }

    [Theory]
    [InlineData("a.txt", "b.TXT")]
    [InlineData("a", "b")]
    public void GetHashCode_EqualExtensions_ReturnsSameHashCode(string x, string y)
    {
        var comparer = FileExtensionEqualityComparer.Instance;

        Assert.Equal(comparer.GetHashCode(x), comparer.GetHashCode(y));
    }

}
