namespace FclEx.Extensions.EnumerableExtensions;

public class IndexExTests
{
    public static readonly IEnumerable<int> Empty = [];

    public static readonly IEnumerable<int> Numbers = Enumerable.Range(1, 10);

    public static readonly TheoryData<IEnumerable<int>> NonEmptyEnumerableCases = new()
    {
        Numbers,
        Numbers.ToList(),
        Numbers.ToArray(),
    };

    public static readonly TheoryData<IEnumerable<int>> EmptyEnumerableCases = new()
    {
        Empty,
        Empty.ToList(),
        Empty.ToArray(),
    };

    [Theory]
    [MemberData(nameof(NonEmptyEnumerableCases))]
    public void IndexEx_NonEmptyEnumerable(IEnumerable<int> enumerable)
    {
        var i = 0;
        var count = Numbers.Count();

        foreach (var (index, item, isFirst, isLast) in enumerable.IndexEx())
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
    public void IndexEx_EmptyEnumerable(IEnumerable<int> enumerable)
    {
        Assert.Empty(enumerable.IndexEx());
    }

    [Fact]
    public void IndexEx_NullEnumerable()
    {
        IEnumerable<int>? enumerable = null;
        Assert.Throws<ArgumentNullException>(() => enumerable!.IndexEx());
    }
}