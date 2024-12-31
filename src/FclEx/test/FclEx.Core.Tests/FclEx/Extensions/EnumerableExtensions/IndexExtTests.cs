namespace FclEx.Extensions.EnumerableExtensions;

public class IndexExtTests
{
    public static IEnumerable<int> Empty { get; } = [];

    public static IEnumerable<int> Numbers { get; } = Enumerable.Range(1, 10);

    public static IEnumerable<object[]> NonEmptyEnumerableCases { get; } =
    [
        [Numbers],
        [Numbers.ToList()],
        [Numbers.ToArray()],
    ];

    public static IEnumerable<object[]> EmptyEnumerableCases { get; } =
    [
        [Empty],
        [Empty.ToList()],
        [Empty.ToArray()],
    ];

    [Theory]
    [MemberData(nameof(NonEmptyEnumerableCases))]
    public void WithIndex_NonEmptyEnumerable(IEnumerable<int> enumerable)
    {
        var i = 0;
        var count = Numbers.Count();

        foreach (var (index, item, isFirst, isLast) in enumerable.IndexExt())
        {
            Assert.Equal(i + 1, item);
            Assert.Equal(i, index);
            Assert.Equal(i == 0, isFirst);
            Assert.Equal(i + 1 == count, isLast);
            ++i;
        }
    }

    [Theory]
    [MemberData(nameof(EmptyEnumerableCases))]
    public void WithIndex_EmptyEnumerable(IEnumerable<int> enumerable)
    {
        Assert.Empty(enumerable.IndexExt());
    }

    [Fact]
    public void WithIndex_NullEnumerable()
    {
        IEnumerable<int>? enumerable = null;
        Assert.Throws<ArgumentNullException>(() => enumerable!.IndexExt());
    }
}