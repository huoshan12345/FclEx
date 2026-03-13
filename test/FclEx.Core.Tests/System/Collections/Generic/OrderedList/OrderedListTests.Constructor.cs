// ReSharper disable all
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
namespace System.Collections.Generic.OrderedList;

public abstract partial class OrderedListTests<T> : IListGenericTests<T>
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
        var enumerable = CreateEnumerable(enumerableType, null, enumerableLength, 0, numberOfDuplicateElements);
        var list = new OrderedList<T>(enumerable);
        var expected = enumerable.ToList();

        Assert.Equal(enumerableLength, list.Count); //"Number of items in list do not match the number of items given."

        for (var i = 0; i < enumerableLength; i++)
            Assert.Equal(expected[i], list[i]); //"Expected object in item array to be the same as in the list"
    }

    [Fact]
    public void Constructo_NullIEnumerable_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => { new OrderedList<T>(null); }); //"Expected ArgumentnUllException for null items"
    }
}