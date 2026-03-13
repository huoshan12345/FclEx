// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable ObjectCreationAsStatement
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
namespace System.Collections.Generic.OrderedList;

public abstract partial class OrderedListTests<T>
{
    [Fact]
    public void Constructor_Default()
    {
        var list = new OrderedList<T>();
        Assert.Empty(list); //"Do not expect anything to be in the list."
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(100)]
    public void Constructor_Capacity(int capacity)
    {
        var list = new OrderedList<T>(capacity);
        Assert.Empty(list); //"Do not expect anything to be in the list."
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrderedList<T>(capacity));
    }

    [Theory]
    [MemberData(nameof(EnumerableTestData))]
    public void Constructor_IEnumerable(EnumerableType enumerableType, int listLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        var enumerable = CreateEnumerable(enumerableType, null!, enumerableLength, 0, numberOfDuplicateElements);
        var list = new OrderedList<T>(enumerable);
        var expected = ToExpectedList(list);

        Assert.Equal(enumerableLength, list.Count);

        for (var i = 0; i < enumerableLength; i++)
            Assert.Equal(expected[i], list[i]);
    }

    [Fact]
    public void Construct_NullIEnumerable_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => { new OrderedList<T>((IEnumerable<T>)null!); });
    }
}