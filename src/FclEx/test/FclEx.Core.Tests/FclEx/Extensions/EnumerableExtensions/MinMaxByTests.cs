using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Extensions.EnumerableExtensions;

public class MinMaxByTests
{
    [Fact]
    public void MinMaxBy_WithEmptySequence_ReturnsNullForReferenceType()
    {
        var source = Enumerable.Empty<string>();
        var result = source.MinMaxBy(x => x.Length);

        Assert.Null(result.Min);
        Assert.Null(result.Max);
    }

    [Fact]
    public void MinMaxBy_WithEmptySequence_ThrowsForValueType()
    {
        var source = Enumerable.Empty<int>();

        Assert.Throws<InvalidOperationException>(() => source.MinMaxBy(x => x));
    }

    [Fact]
    public void MinMaxBy_WithSingleElement_ReturnsThatElementAsBothMinAndMax()
    {
        var source = new[] { "apple" };
        var result = source.MinMaxBy(x => x.Length);

        Assert.Equal("apple", result.Min);
        Assert.Equal("apple", result.Max);
    }

    [Fact]
    public void MinMaxBy_WithMultipleElements_ReturnsCorrectMinAndMax()
    {
        var source = new[] { "apple", "banana", "cherry", "date" };
        var result = source.MinMaxBy(x => x.Length);

        Assert.Equal("date", result.Min);  // Shortest string
        Assert.Equal("banana", result.Max); // Longest string
    }

    [Fact]
    public void MinMaxBy_WithDescendingOrder_ReturnsLastAsMinAndFirstAsMax()
    {
        var source = new[] { 100, 80, 60, 40, 20 };
        var result = source.MinMaxBy(x => x);

        Assert.Equal(20, result.Min); // Smallest element
        Assert.Equal(100, result.Max); // Largest element
    }

    [Fact]
    public void MinMaxBy_WithCustomComparer_ReturnsCorrectMinAndMax()
    {
        var source = new[] { "apple", "banana", "cherry", "date" };
        var result = source.MinMaxBy(x => x, StringComparer.OrdinalIgnoreCase);

        Assert.Equal("apple", result.Min);  // Alphabetically first
        Assert.Equal("date", result.Max); // Alphabetically last
    }

    [Fact]
    public void MinMaxBy_WithNegativeNumbers_ReturnsCorrectMinAndMax()
    {
        var source = new[] { -5, -10, 0, 5, 10 };
        var result = source.MinMaxBy(x => x);

        Assert.Equal(-10, result.Min); // Lowest number
        Assert.Equal(10, result.Max);  // Highest number
    }

    [Fact]
    public void MinMaxBy_WithCustomKeySelector_ReturnsCorrectMinAndMax()
    {
        var source = new[] { "apple", "banana", "cherry", "date" };
        var result = source.MinMaxBy(x => x[0]); // Key based on first character

        Assert.Equal("apple", result.Min);  // 'a' is the smallest starting character
        Assert.Equal("date", result.Max);   // 'd' is the largest starting character
    }
}