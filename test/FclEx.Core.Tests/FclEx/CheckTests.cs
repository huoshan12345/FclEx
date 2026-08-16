#pragma warning disable IDE0028 // Simplify collection initialization

namespace FclEx;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
public class CheckTests
{
    [Fact]
    public void TryGetSingleNonNull_BothNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Check.TryGetSingleNonNull<string>(null, null, out _));
    }

    [Theory]
    [InlineData(null, "right", "right")]
    [InlineData("left", null, "left")]
    public void TryGetSingleNonNull_ExactlyOneNull_ReturnsNonNullValue(string? left, string? right, string expected)
    {
        var success = Check.TryGetSingleNonNull(left, right, out var result);

        Assert.True(success);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryGetSingleNonNull_BothNonNull_ReturnsFalse()
    {
        var success = Check.TryGetSingleNonNull("left", "right", out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(null, "right", 5)]
    [InlineData("left", null, 4)]
    [InlineData("left", "right", 9)]
    public void TryGetSingleNonNull_NullabilityAttributesSupportBothReturnBranches(
        string? left,
        string? right,
        int expectedLength)
    {
        Assert.Equal(expectedLength, GetLength(left, right));
    }

    private static int GetLength(string? left, string? right)
    {
        if (Check.TryGetSingleNonNull(left, right, out var result))
            return result.Length;

        return left.Length + right.Length;
    }

    [Fact]
    public void NotEmpty_List()
    {
        {
            var col = new List<int>();
            var ex = Assert.Throws<ArgumentException>(() => Check.NotEmpty(col));
            Assert.Contains($"The list argument '{nameof(col)}' cannot be empty.", ex.Message);
        }
        {
            var col = new List<int> { 1 };
            Check.NotEmpty(col);
        }
    }

    [Fact]
    public void NotEmpty_Array()
    {
        {
            var col = Array.Empty<int>();
            var ex = Assert.Throws<ArgumentException>(() => Check.NotEmpty(col));
            Assert.Contains($"The list argument '{nameof(col)}' cannot be empty.", ex.Message);
        }
        {
            var col = new int[] { 1 };
            Check.NotEmpty(col);
        }
    }

    [Fact]
    public void NotEmpty_IList()
    {
        {
            IList<int> col = new List<int>();
            var ex = Assert.Throws<ArgumentException>(() => Check.NotEmpty(col));
            Assert.Contains($"The list argument '{nameof(col)}' cannot be empty.", ex.Message);
        }
        {
            IList<int> col = new List<int> { 1 };
            Check.NotEmpty(col);
        }
    }

    [Fact]
    public void NotEmpty_IReadOnlyList()
    {
        {
            IReadOnlyList<int> col = new List<int>();
            var ex = Assert.Throws<ArgumentException>(() => Check.NotEmpty(col));
            Assert.Contains($"The list argument '{nameof(col)}' cannot be empty.", ex.Message);
        }
        {
            IReadOnlyList<int> col = new List<int> { 1 };
            Check.NotEmpty(col);
        }
    }

    [Fact]
    public void NotEmpty_ICollection()
    {
        {
            ICollection<int> col = new List<int>();
            var ex = Assert.Throws<ArgumentException>(() => Check.NotEmpty(col));
            Assert.Contains($"The list argument '{nameof(col)}' cannot be empty.", ex.Message);
        }
        {
            ICollection<int> col = new List<int> { 1 };
            Check.NotEmpty(col);
        }
    }

    [Fact]
    public void NotEmpty_IReadOnlyCollection()
    {
        {
            IReadOnlyCollection<int> col = new List<int>();
            var ex = Assert.Throws<ArgumentException>(() => Check.NotEmpty(col));
            Assert.Contains($"The list argument '{nameof(col)}' cannot be empty.", ex.Message);
        }
        {
            IReadOnlyCollection<int> col = new List<int> { 1 };
            Check.NotEmpty(col);
        }
    }

    [Fact]
    public void NotEmpty_HashSet()
    {
        {
            var col = new HashSet<int>();
            var ex = Assert.Throws<ArgumentException>(() => Check.NotEmpty(col));
            Assert.Contains($"The list argument '{nameof(col)}' cannot be empty.", ex.Message);
        }
        {
            var col = new HashSet<int> { 1 };
            Check.NotEmpty(col);
        }
    }

    [Fact]
    public void NotEmpty_ISet()
    {
        {
            ISet<int> col = new HashSet<int>();
            var ex = Assert.Throws<ArgumentException>(() => Check.NotEmpty(col));
            Assert.Contains($"The list argument '{nameof(col)}' cannot be empty.", ex.Message);
        }
        {
            ISet<int> col = new HashSet<int> { 1 };
            Check.NotEmpty(col);
        }
    }

#if NET5_0_OR_GREATER
    [Fact]
    public void NotEmpty_IReadOnlySet()
    {
        {
            IReadOnlySet<int> col = new HashSet<int>();
            var ex = Assert.Throws<ArgumentException>(() => Check.NotEmpty(col));
            Assert.Contains($"The list argument '{nameof(col)}' cannot be empty.", ex.Message);
        }
        {
            IReadOnlySet<int> col = new HashSet<int> { 1 };
            Check.NotEmpty(col);
        }
    }
#endif
}
