namespace FclEx.Extensions;

public class PathExtensionsTests
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
        var newName = Path.GetNextFileName(fileName);
        Assert.Equal(expected, newName);
    }
}
