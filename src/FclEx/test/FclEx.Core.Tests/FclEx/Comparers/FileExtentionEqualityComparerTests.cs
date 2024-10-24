namespace FclEx.Comparers;

public class FileExtentionEqualityComparerTests
{
    [Theory]
    [InlineData("a.txt", "b.txt", true)]
    [InlineData("a.txt", "b .txt", true)]
    [InlineData("a.txt", ".txt", true)]
    [InlineData("a.txt", "b.t xt", false)]
    [InlineData("a.txt", "btxt", false)]
    [InlineData("a.txt", "b. txt", false)]
    [InlineData("a.txt", null, false)]
    [InlineData(null, null, true)]
    public void Equals_Test(string? x, string? y, bool equal)
    {
        Assert.Equal(equal, FileExtensionEqualityComparer.Instance.Equals(x, y));
    }

}