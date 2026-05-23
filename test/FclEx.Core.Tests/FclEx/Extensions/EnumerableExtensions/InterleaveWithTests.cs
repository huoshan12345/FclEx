#pragma warning disable IDE0059
#pragma warning disable CA1861

namespace FclEx.Extensions.EnumerableExtensions;

public class InterleaveWithTests
{
    [Fact]
    public void BothSequencesEmpty_ReturnsEmpty()
    {
        var first = Array.Empty<int>();
        var second = Array.Empty<int>();
        var result = first.InterleaveWith(second, 1, 1).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void InterleavesWithGroupingOfOne()
    {
        var first = new[] { 1, 3, 5 };
        var second = new[] { 2, 4, 6 };
        var result = first.InterleaveWith(second, 1, 1).ToList();
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, result);
    }

    [Fact]
    public void InterleavesWithDifferentGroupSizes()
    {
        var first = new[] { 1, 2, 3, 4 };
        var second = new[] { 10, 11, 12, 13 };
        var result = first.InterleaveWith(second, 2, 1).ToList();
        Assert.Equal(new[] { 1, 2, 10, 3, 4, 11, 12, 13 }, result);
    }

    [Fact]
    public void FirstSequenceRunsOut_FirstIsShorter()
    {
        var first = new[] { 1 };
        var second = new[] { 2, 3, 4, 5 };
        var result = first.InterleaveWith(second, 2, 1).ToList();
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [Fact]
    public void SecondSequenceRunsOut_SecondIsShorter()
    {
        var first = new[] { 1, 2, 3, 4, 5 };
        var second = new[] { 10 };
        var result = first.InterleaveWith(second, 2, 2).ToList();
        Assert.Equal(new[] { 1, 2, 10, 3, 4, 5 }, result);
    }

    [Fact]
    public void UnevenFinalGroup_IsEmittedPartially()
    {
        var first = new[] { 1, 2, 3 };
        var second = new[] { 10, 11 };
        var result = first.InterleaveWith(second, 2, 2).ToList();
        Assert.Equal(new[] { 1, 2, 10, 11, 3 }, result);
    }

    [Fact]
    public void IsLazy()
    {
        var enumerated = false;

        IEnumerable<int> First()
        {
            enumerated = true;
            yield return 1;
            yield return 2;
        }

        var second = new[] { 3, 4 };

        var result = First().InterleaveWith(second, 1, 1);

        Assert.False(enumerated);

        _ = result.First();

        Assert.True(enumerated);
    }

    [Fact]
    public void NullFirst_Throws()
    {
        IEnumerable<int> first = null!;
        var second = new[] { 1 };

        Assert.Throws<ArgumentNullException>(() => first.InterleaveWith(second, 1, 1).ToList());
    }

    [Fact]
    public void NullSecond_Throws()
    {
        var first = new[] { 1 };
        IEnumerable<int> second = null!;

        Assert.Throws<ArgumentNullException>(() => first.InterleaveWith(second, 1, 1).ToList());
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    public void InvalidGrouping_Throws(int firstGrouping, int secondGrouping)
    {
        var first = new[] { 1 };
        var second = new[] { 2 };

        Assert.Throws<ArgumentOutOfRangeException>(() => first.InterleaveWith(second, firstGrouping, secondGrouping).ToList());
    }
}