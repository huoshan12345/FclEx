using System.Collections;
using System.Collections.ObjectModel;

namespace FclEx.Extensions.EnumerableExtensions;

public class FindIndexTests
{
    public static readonly TheoryData<IEnumerable<int>> SourceCases = new()
    {
        Yield(10, 20, 30, 40),
        new List<int> { 10, 20, 30, 40 },
        new[] { 10, 20, 30, 40 },
        new ReadOnlyCollection<int>(new List<int> { 10, 20, 30, 40 }),
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

    [Fact]
    public void FindIndex_WithKnownCountSourceAndCountPastEnd_ShouldThrowBeforeEnumerating()
    {
        var source = new ThrowingCollection<int>([1, 2, 3]);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => source.FindIndex(2, 2, _ => true));

        Assert.Equal("count", exception.ParamName);
    }

    [Fact]
    public void FindIndex_WithUnknownCountSourceAndValidRange_ShouldNotEnumeratePastRange()
    {
        var source = YieldWithTailThatThrows(1, 2);

        var index = source.FindIndex(0, 2, m => m == 3);

        Assert.Equal(-1, index);
    }

    private static IEnumerable<int> Yield(params int[] values)
    {
        foreach (var value in values)
        {
            yield return value;
        }
    }

    private static IEnumerable<int> YieldWithTailThatThrows(params int[] values)
    {
        foreach (var value in values)
        {
            yield return value;
        }

        throw new InvalidOperationException("The sequence was enumerated past the requested range.");
    }

    private sealed class ThrowingCollection<T>(IReadOnlyList<T> items) : ICollection<T>
    {
        public int Count => items.Count;

        public bool IsReadOnly => true;

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new InvalidOperationException("The sequence should not be enumerated.");
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
