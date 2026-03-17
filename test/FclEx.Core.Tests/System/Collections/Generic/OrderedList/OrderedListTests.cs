namespace System.Collections.Generic.OrderedList;

public abstract partial class OrderedListTests<T> : IList_Generic_Tests<T>
{
    #region IList<T> Helper Methods
    protected override bool Enumerator_Empty_UsesSingletonInstance => true;
    protected override bool Enumerator_Empty_Current_UndefinedOperation_Throws => true;
    protected override bool Enumerator_Empty_ModifiedDuringEnumeration_ThrowsInvalidOperationException => false;

    protected override IList<T> GenericIListFactory()
    {
        return GenericListFactory();
    }

    protected override IList<T> GenericIListFactory(int count)
    {
        return GenericListFactory(count);
    }

    #endregion

    #region List<T> Helper Methods

    protected virtual OrderedList<T> GenericListFactory()
    {
        return [];
    }

    protected virtual OrderedList<T> GenericListFactory(int count)
    {
        var toCreateFrom = CreateEnumerable(EnumerableType.List, null, count, 0, 0);
        return new OrderedList<T>(toCreateFrom);
    }

    protected void VerifyList(IList<T> list, IList<T> expectedItems)
    {
        Assert.Equal(expectedItems.Count, list.Count);

        //Only verify the indexer. List should be in a good enough state that we
        //do not have to verify consistency with any other method.
        for (var i = 0; i < list.Count; ++i)
        {
            Assert.True(list[i] is { } item
                ? item.Equals(expectedItems[i])
                : expectedItems[i] == null);
        }
    }

    #endregion

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void CopyTo_ArgumentValidity(int count)
    {
        var list = GenericListFactory(count);
        AssertExtensions.Throws<ArgumentException>(null, () => list.CopyTo(0, [], 0, count + 1));
        AssertExtensions.Throws<ArgumentException>(null, () => list.CopyTo(count, [], 0, 1));
    }
}