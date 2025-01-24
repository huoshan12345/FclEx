namespace FclEx.Extensions.EnumerableExtensions;

public class MinimaByTests
{
    [Fact]
    public void MinimaBy_WithEmptySequence_ReturnsEmptyList()
    {
        var source = Enumerable.Empty<int>();
        var result = source.MinimaBy(x => x);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public void MinimaBy_WithSingleElement_ReturnsThatElement()
    {
        var source = new[] { "apple" };
        var result = source.MinimaBy(x => x.Length);

        Assert.Single(result.Items, "apple");
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public void MinimaBy_WithMultipleElements_ReturnsCorrectMinima()
    {
        var source = new[] { "apple", "banana", "cherry", "date", "fig" };
        var result = source.MinimaBy(x => x.Length);

        Assert.Equal(["fig"], result.Items);
        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public void MinimaBy_WithMultipleEqualMinima_ReturnsAllEqualMinima()
    {
        var source = new[] { "apple", "fig", "banana", "cherry", "date", "fig" };
        var result = source.MinimaBy(x => x.Length);

        Assert.Equal(["fig", "fig"], result.Items); // Two equal minima
        Assert.Equal(6, result.TotalCount);
    }

    [Fact]
    public void MinimaBy_WithCustomComparer_ReturnsCorrectMinima()
    {
        var source = new[] { "apple", "Apple", "cherry", "date" };
        var result = source.MinimaBy(x => x, StringComparer.OrdinalIgnoreCase); // Case-insensitive

        Assert.Equal(["apple", "Apple"], result.Items); // Both have the same value in a case-insensitive comparison
        Assert.Equal(4, result.TotalCount);
    }
}