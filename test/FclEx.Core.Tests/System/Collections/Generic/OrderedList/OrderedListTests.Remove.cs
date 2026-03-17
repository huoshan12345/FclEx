// ReSharper disable UnusedVariable
// ReSharper disable ConvertToLocalFunction
namespace System.Collections.Generic.OrderedList;

public abstract partial class OrderedListTests<T>
{
    #region RemoveAll(Pred<T>)

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void RemoveAll_AllElements(int count)
    {
        var list = GenericListFactory(count);
        var beforeList = list.ToList();
        var removedCount = list.RemoveAll(_ => true);
        Assert.Equal(count, removedCount);
        Assert.Equal(0, list.Count);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void RemoveAll_NoElements(int count)
    {
        var list = GenericListFactory(count);
        var beforeList = list.ToList();
        var removedCount = list.RemoveAll(_ => false);
        Assert.Equal(0, removedCount);
        Assert.Equal(count, list.Count);
        VerifyList(list, beforeList);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void RemoveAll_DefaultElements(int count)
    {
        var list = GenericListFactory(count);
        var beforeList = list.ToList();
        Predicate<T> equalsDefaultElement = value => default(T) == null 
            ? value == null 
            : default(T)!.Equals(value);

        var expectedCount = beforeList.Count(value => equalsDefaultElement(value));
        var removedCount = list.RemoveAll(equalsDefaultElement);
        Assert.Equal(expectedCount, removedCount);
    }

    [Fact]
    public void RemoveAll_NullMatchPredicate()
    {
        AssertExtensions.Throws<ArgumentNullException>("match", () => new List<T>().RemoveAll(null!));
    }

    #endregion

    #region RemoveRange

    // TODO: Add tests for RemoveRange

    #endregion
}