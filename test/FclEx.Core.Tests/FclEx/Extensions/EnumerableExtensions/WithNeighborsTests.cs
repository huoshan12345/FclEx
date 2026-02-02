#pragma warning disable IDE0059

namespace FclEx.Extensions.EnumerableExtensions;

public class WithNeighborsTests
{
    [Fact]
    public void WithNeighbors_EmptySequence_ReturnsEmpty()
    {
        var source = Array.Empty<int>();
        var result = source.WithNeighbors().ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void WithNeighbors_SingleElement_HasNoNeighbors()
    {
        var source = new[] { 42 };
        var result = source.WithNeighbors().Single();
        Assert.Equal(42, result.Item);
        Assert.Equal(default, result.Previous);
        Assert.Equal(default, result.Next);
    }

    [Fact]
    public void WithNeighbors_TwoElements_AssignsNeighborsCorrectly()
    {
        var source = new[] { 1, 2 };
        var result = source.WithNeighbors().ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal((1, default, 2), result[0]);
        Assert.Equal((2, 1, default), result[1]);
    }

    [Fact]
    public void WithNeighbors_MultipleElements_AssignsPreviousAndNext()
    {
        var source = new[] { 1, 2, 3, 4 };
        var result = source.WithNeighbors().ToList();

        Assert.Equal(4, result.Count);
        Assert.Equal((1, default, 2), result[0]);
        Assert.Equal((2, 1, 3), result[1]);
        Assert.Equal((3, 2, 4), result[2]);
        Assert.Equal((4, 3, default), result[3]);
    }
    
    [Fact]
    public void WithNeighbors_ReferenceType_AllowsNullNeighbors()
    {
        var source = new[] { "a", "b", "c" };
        var result = source.WithNeighbors().ToList();

        Assert.Null(result[0].Previous);
        Assert.Equal("b", result[0].Next);
        Assert.Equal("a", result[1].Previous);
        Assert.Equal("c", result[1].Next);
        Assert.Equal("b", result[2].Previous);
        Assert.Null(result[2].Next);
    }
    
    [Fact]
    public void WithNeighbors_IsLazy()
    {
        var enumerated = false;

        IEnumerable<int> Source()
        {
            enumerated = true;
            yield return 1;
            yield return 2;
        }

        var result = Source().WithNeighbors();

        Assert.False(enumerated);

        _ = result.First();

        Assert.True(enumerated);
    }
    
    [Fact]
    public void WithNeighbors_NullSource_Throws()
    {
        IEnumerable<int> source = null!;
        Assert.Throws<ArgumentNullException>(() => source.WithNeighbors().ToList());
    }

    [Fact]
    public void WithPrevious_EmptySequence_ReturnsEmpty()
    {
        var source = Array.Empty<int>();

        var result = source.WithPrevious().ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void WithPrevious_SingleElement_PreviousIsDefault()
    {
        var source = new[] { 42 };

        var result = source.WithPrevious().Single();

        Assert.Equal(42, result.Item);
        Assert.Equal(default, result.Previous);
    }

    [Fact]
    public void WithPrevious_MultipleElements_AssignsPreviousCorrectly()
    {
        var source = new[] { 1, 2, 3, 4 };

        var result = source.WithPrevious().ToList();

        Assert.Equal(4, result.Count);

        Assert.Equal((1, default), result[0]);
        Assert.Equal((2, 1), result[1]);
        Assert.Equal((3, 2), result[2]);
        Assert.Equal((4, 3), result[3]);
    }

    [Fact]
    public void WithPrevious_ReferenceType_AllowsNullPrevious()
    {
        var source = new[] { "a", "b", "c" };

        var result = source.WithPrevious().ToList();

        Assert.Null(result[0].Previous);
        Assert.Equal("a", result[1].Previous);
        Assert.Equal("b", result[2].Previous);
    }

    [Fact]
    public void WithPrevious_IsLazy()
    {
        var enumerated = false;

        IEnumerable<int> Source()
        {
            enumerated = true;
            yield return 1;
            yield return 2;
        }

        var result = Source().WithPrevious();

        Assert.False(enumerated);

        _ = result.First();

        Assert.True(enumerated);
    }

    [Fact]
    public void WithPrevious_NullSource_Throws()
    {
        IEnumerable<int> source = null!;

        Assert.Throws<ArgumentNullException>(() => source.WithPrevious().ToList());
    }

    [Fact]
    public void EmptySequence_ReturnsEmpty()
    {
        var source = Array.Empty<int>();
        var result = source.WithNext().ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void SingleElement_NextIsDefault()
    {
        var source = new[] { 42 };
        var result = source.WithNext().Single();
        Assert.Equal(42, result.Item);
        Assert.Equal(default, result.Next);
    }

    [Fact]
    public void TwoElements_AssignsNextCorrectly()
    {
        var source = new[] { 1, 2 };
        var result = source.WithNext().ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal((1, 2), result[0]);
        Assert.Equal((2, default), result[1]);
    }

    [Fact]
    public void MultipleElements_AssignsNextCorrectly()
    {
        var source = new[] { 1, 2, 3, 4 };
        var result = source.WithNext().ToList();

        Assert.Equal(4, result.Count);
        Assert.Equal((1, 2), result[0]);
        Assert.Equal((2, 3), result[1]);
        Assert.Equal((3, 4), result[2]);
        Assert.Equal((4, default), result[3]);
    }

    [Fact]
    public void ReferenceType_AllowsNullNext()
    {
        var source = new[] { "a", "b", "c" };
        var result = source.WithNext().ToList();

        Assert.Equal("b", result[0].Next);
        Assert.Equal("c", result[1].Next);
        Assert.Null(result[2].Next);
    }

    [Fact]
    public void IsLazy()
    {
        var enumerated = false;

        IEnumerable<int> Source()
        {
            enumerated = true;
            yield return 1;
            yield return 2;
        }

        var result = Source().WithNext();

        Assert.False(enumerated);

        _ = result.First();

        Assert.True(enumerated);
    }

    [Fact]
    public void NullSource_Throws()
    {
        IEnumerable<int> source = null!;
        Assert.Throws<ArgumentNullException>(() => source.WithNext().ToList());
    }
}
