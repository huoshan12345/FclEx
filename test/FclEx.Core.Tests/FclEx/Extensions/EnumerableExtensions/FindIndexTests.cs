namespace FclEx.Extensions.EnumerableExtensions;

public class FindIndexTests
{
    public static readonly TheoryData<IEnumerable<int>> SourceCases = new()
    {
        Yield(10, 20, 30, 40),
        new List<int> { 10, 20, 30, 40 },
        new[] { 10, 20, 30, 40 },
    };

    [Theory]
    [MemberData(nameof(SourceCases))]
    public void FindIndex_WhenMatchExists_ShouldReturnIndex(IEnumerable<int> source)
    {
        var index = source.FindIndex(m => m == 30);
        Assert.Equal(2, index);
    }

    [Theory]
    [MemberData(nameof(SourceCases))]
    public void FindIndex_WhenMatchDoesNotExist_ShouldReturnMinusOne(IEnumerable<int> source)
    {
        var index = source.FindIndex(m => m == 50);
        Assert.Equal(-1, index);
    }

    [Theory]
    [MemberData(nameof(SourceCases))]
    public void FindIndex_WithStartIndex_ShouldReturnAbsoluteIndex(IEnumerable<int> source)
    {
        var index = source.FindIndex(2, m => m == 40);
        Assert.Equal(3, index);
    }

    [Theory]
    [MemberData(nameof(SourceCases))]
    public void FindIndex_WithStartIndex_ShouldIgnoreEarlierMatches(IEnumerable<int> source)
    {
        var index = source.FindIndex(2, m => m == 20);
        Assert.Equal(-1, index);
    }

    [Theory]
    [MemberData(nameof(SourceCases))]
    public void FindIndex_WithStartIndexAtEnd_ShouldReturnMinusOne(IEnumerable<int> source)
    {
        var predicateCalled = false;
        var index = source.FindIndex(4, _ =>
        {
            predicateCalled = true;
            return true;
        });

        Assert.Equal(-1, index);
        Assert.False(predicateCalled);
    }

    [Theory]
    [MemberData(nameof(SourceCases))]
    public void FindIndex_WithStartIndexAndCount_ShouldReturnAbsoluteIndex(IEnumerable<int> source)
    {
        var index = source.FindIndex(1, 3, m => m == 40);
        Assert.Equal(3, index);
    }

    [Theory]
    [MemberData(nameof(SourceCases))]
    public void FindIndex_WithStartIndexAndCount_ShouldSearchOnlySpecifiedRange(IEnumerable<int> source)
    {
        var index = source.FindIndex(1, 2, m => m == 40);
        Assert.Equal(-1, index);
    }

    [Theory]
    [MemberData(nameof(SourceCases))]
    public void FindIndex_WithZeroCount_ShouldReturnMinusOne(IEnumerable<int> source)
    {
        var predicateCalled = false;
        var index = source.FindIndex(2, 0, _ =>
        {
            predicateCalled = true;
            return true;
        });

        Assert.Equal(-1, index);
        Assert.False(predicateCalled);
    }

    [Fact]
    public void FindIndex_WhenSourceIsNull_ShouldThrowArgumentNullException()
    {
        IEnumerable<int>? source = null;
        var exception = Assert.Throws<ArgumentNullException>(() => source!.FindIndex(m => m == 1));
        Assert.Equal("source", exception.ParamName);
    }

    [Fact]
    public void FindIndex_WhenMatchIsNull_ShouldThrowArgumentNullException()
    {
        Predicate<int>? match = null;
        var exception = Assert.Throws<ArgumentNullException>(() => Yield(1).FindIndex(match!));
        Assert.Equal("match", exception.ParamName);
    }

    [Fact]
    public void FindIndex_WithNegativeStartIndex_ShouldThrowArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Yield(1).FindIndex(-1, m => m == 1));
        Assert.Equal("startIndex", exception.ParamName);
    }

    [Fact]
    public void FindIndex_WithStartIndexPastEnd_ShouldThrowArgumentOutOfRangeException()
    {
        var predicateCalled = false;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Yield(1, 2).FindIndex(3, _ =>
        {
            predicateCalled = true;
            return true;
        }));

        Assert.Equal("startIndex", exception.ParamName);
        Assert.False(predicateCalled);
    }

    [Fact]
    public void FindIndex_WithNegativeCount_ShouldThrowArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Yield(1).FindIndex(0, -1, m => m == 1));
        Assert.Equal("count", exception.ParamName);
    }

    [Fact]
    public void FindIndex_WithCountPastEnd_ShouldThrowArgumentOutOfRangeException()
    {
        var predicateCalled = false;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Yield(1, 2, 3).FindIndex(2, 2, _ =>
        {
            predicateCalled = true;
            return true;
        }));

        Assert.Equal("count", exception.ParamName);
        Assert.False(predicateCalled);
    }

    private static IEnumerable<int> Yield(params int[] values)
    {
        foreach (var value in values)
        {
            yield return value;
        }
    }
}
