namespace System.Collections.Generic;

public class EqualityComparerBuilderTests
{
    [Fact]
    public void FileExtension_ShouldBuildFileExtensionComparer()
    {
        var comparer = EqualityComparerBuilder
            .For<string>()
            .FileExtension()
            .Build();

        Assert.True(comparer.Equals("a.txt", "b.TXT"));
        Assert.Equal(comparer.GetHashCode("a.txt"), comparer.GetHashCode("b.TXT"));
    }

    [Fact]
    public void Common_ShouldBuildDelegateEqualityComparer()
    {
        var comparer = EqualityComparerBuilder
            .For<int>()
            .Common((x, y) => x % 2 == y % 2, x => x % 2)
            .Build();

        Assert.True(comparer.Equals(1, 3));
        Assert.False(comparer.Equals(1, 2));
        Assert.Equal(comparer.GetHashCode(1), comparer.GetHashCode(3));
    }
}
