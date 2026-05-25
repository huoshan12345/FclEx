namespace FclEx.Extensions;

partial class ArrayExtensionsTests
{
    [Fact]
    public void FindIndex_WhenMatchExists_ShouldReturnFirstMatchedIndex()
    {
        int[] array = [10, 20, 30, 20];

        var index = array.FindIndex(m => m == 20);

        Assert.Equal(1, index);
    }

    [Fact]
    public void FindIndex_WhenMatchDoesNotExist_ShouldReturnMinusOne()
    {
        int[] array = [10, 20, 30];

        var index = array.FindIndex(m => m == 40);

        Assert.Equal(-1, index);
    }

    [Fact]
    public void FindIndex_WithStartIndex_ShouldIgnoreEarlierMatches()
    {
        int[] array = [10, 20, 30, 20];

        var index = array.FindIndex(2, m => m == 20);

        Assert.Equal(3, index);
    }

    [Fact]
    public void FindIndex_WithStartIndexAndCount_ShouldSearchOnlyRange()
    {
        int[] array = [10, 20, 30, 20];

        var index = array.FindIndex(1, 2, m => m == 20);

        Assert.Equal(1, index);
    }

    [Fact]
    public void FindIndex_WithZeroCount_ShouldReturnMinusOne()
    {
        int[] array = [10, 20, 30];
        var called = false;

        var index = array.FindIndex(1, 0, _ =>
        {
            called = true;
            return true;
        });

        Assert.Equal(-1, index);
        Assert.False(called);
    }

    [Fact]
    public void FindIndex_WhenMatchIsNull_ShouldThrowArgumentNullException()
    {
        int[] array = [10];
        Predicate<int>? match = null;

        var exception = Assert.Throws<ArgumentNullException>(() => array.FindIndex(match!));

        Assert.Equal("match", exception.ParamName);
    }

    [Fact]
    public void FindLastIndex_WhenMatchExists_ShouldReturnLastMatchedIndex()
    {
        int[] array = [10, 20, 30, 20];

        var index = array.FindLastIndex(m => m == 20);

        Assert.Equal(3, index);
    }

    [Fact]
    public void FindLastIndex_WhenMatchDoesNotExist_ShouldReturnMinusOne()
    {
        int[] array = [10, 20, 30];

        var index = array.FindLastIndex(m => m == 40);

        Assert.Equal(-1, index);
    }

    [Fact]
    public void FindLastIndex_WithStartIndex_ShouldIgnoreLaterMatches()
    {
        int[] array = [10, 20, 30, 20];

        var index = array.FindLastIndex(2, m => m == 20);

        Assert.Equal(1, index);
    }

    [Fact]
    public void FindLastIndex_WithStartIndexAndCount_ShouldSearchOnlyRange()
    {
        int[] array = [10, 20, 30, 20];

        var index = array.FindLastIndex(2, 2, m => m == 20);

        Assert.Equal(1, index);
    }

    [Fact]
    public void FindLastIndex_WithZeroCount_ShouldReturnMinusOne()
    {
        int[] array = [10, 20, 30];
        var called = false;

        var index = array.FindLastIndex(1, 0, _ =>
        {
            called = true;
            return true;
        });

        Assert.Equal(-1, index);
        Assert.False(called);
    }

    [Fact]
    public void FindLastIndex_WhenMatchIsNull_ShouldThrowArgumentNullException()
    {
        int[] array = [10];
        Predicate<int>? match = null;

        var exception = Assert.Throws<ArgumentNullException>(() => array.FindLastIndex(match!));

        Assert.Equal("match", exception.ParamName);
    }
}
