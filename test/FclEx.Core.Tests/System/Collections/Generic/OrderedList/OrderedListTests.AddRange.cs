// ReSharper disable PossibleMultipleEnumeration
namespace System.Collections.Generic.OrderedList;

public abstract partial class OrderedListTests<T>
{
    // Has tests that pass a variably sized TestCollection and MyEnumerable to the AddRange function
    [Theory]
    [MemberData(nameof(EnumerableTestData))]
    public void AddRange(EnumerableType enumerableType, int listLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        var list = GenericListFactory(listLength);
        var enumerable = CreateEnumerable(enumerableType, list, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
        list.AddRange(enumerable);
        var expectedList = ToExpectedList(list);

        // Check that the first section of the List is unchanged
        Assert.All(Enumerable.Range(0, expectedList.Count), index =>
        {
            Assert.Equal(expectedList[index], list[index]);
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void AddRange_NullEnumerable_ThrowsArgumentNullException(int count)
    {
        var list = GenericListFactory(count);
        var listBeforeAdd = ToExpectedList(list);
        Assert.Throws<ArgumentNullException>(() => list.AddRange(null!));
        Assert.Equal(listBeforeAdd, list);
    }
}