namespace FclEx.Extensions.EnumerableExtensions;

public class MaximaByTests
{
    [Fact]
    public void MaximaBy_WithEmptySequence_ReturnsEmptyList()
    {
        var source = Enumerable.Empty<int>();
        var result = source.MaximaBy(x => x);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public void MaximaBy_WithSingleElement_ReturnsThatElement()
    {
        var source = new[] { "apple" };
        var result = source.MaximaBy(x => x.Length);

        Assert.Single(result.Items, "apple");
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public void MaximaBy_WithMultipleElements_ReturnsCorrectMaxima()
    {
        var source = new[] { "apple", "banana", "cherry", "date", "fig" };
        var result = source.MaximaBy(x => x.Length);

        Assert.Equal(["banana", "cherry"], result.Items); // Longest string
        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public void MaximaBy_WithMultipleEqualMaxima_ReturnsAllEqualMaxima()
    {
        var source = new[] { "banana", "apple", "date", "banana", "fig" };
        var result = source.MaximaBy(x => x.Length);

        Assert.Equal(["banana", "banana"], result.Items); // Two equal maxima
        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public void MaximaBy_WithCustomComparer_ReturnsCorrectMaxima()
    {
        var source = new[] { "apple", "Date", "cherry", "date" };
        var result = source.MaximaBy(x => x, StringComparer.OrdinalIgnoreCase); // Case-insensitive

        Assert.Equal(["Date", "date"], result.Items); // Case-insensitive comparison finds "Banana"
        Assert.Equal(4, result.TotalCount);
    }
}