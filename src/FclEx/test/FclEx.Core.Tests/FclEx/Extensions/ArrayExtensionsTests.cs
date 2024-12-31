namespace FclEx.Extensions;

public class ArrayExtensionsTests
{
    [Fact]
    public void Segments_Test()
    {
        var arr = Enumerable.Range(1, 10).ToArray();
        var size = 4;
        var segments = arr.Segments(size).ToList();

        Assert.Equal(0, segments[0].Offset);
        Assert.Equal(size, segments[0].Count);

        Assert.Equal(4, segments[1].Offset);
        Assert.Equal(size, segments[1].Count);

        Assert.Equal(8, segments[2].Offset);
        Assert.Equal(2, segments[2].Count);
    }

    [Fact]
    public void Segments_Null_Test()
    {
        int[]? arr = null;
        Assert.Empty(arr.Segments(4));
    }

    [Fact]
    public void Segments_InvalidSize_Test()
    {
        var arr = Enumerable.Range(1, 10).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => arr.Segments(0).ToList());
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void Concat_Test(int length)
    {
        var arr = Enumerable.Range(1, length)
            .Select(m => Enumerable.Range(1, m))
            .Concat()
            .ToArray();

        var index = 0;
        for (var i = 0; i < length; i++)
        {
            for (var j = 0; j < i; j++)
            {
                Assert.Equal(j + 1, arr[index + j]);
            }
            index += i;
        }
    }

    [Fact]
    public void Concat_With_Test()
    {
        var x = new[] { 1, 2 };
        var y = new[] { 3, 4, 5 };
        var z = x.Concat(y);
        Assert.Equal([1, 2, 3, 4, 5], z);
    }
}