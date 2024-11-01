namespace FclEx.Extensions.BytesExtensions;

public class IndexOfTests
{
    [Theory]
    [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 1, 3 }, -1)]
    [InlineData(new byte[] { 1, 2, 3 }, new byte[] { }, -1)]
    [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 1 }, 0)]
    [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 2, 3 }, 1)]
    [InlineData(new byte[] { 1, 2, 3, 20, 30, 40 }, new byte[] { 20, 30, 40 }, 3)]
    [InlineData(new byte[] { 1, 2 }, new byte[] { 1, 2, 3 }, -1)]
    [InlineData(new byte[] { }, new byte[] { 1, 2, 3 }, -1)]
    [InlineData(new byte[] { }, new byte[] { }, -1)]
    public void IndexOf(byte[] array, byte[] subArray, int expectedIndex)
    {
        Assert.Equal(expectedIndex, array.IndexOf(subArray));
    }
}