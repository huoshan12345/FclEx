namespace System.Collections.Generic.OrderedList;

public abstract partial class OrderedListTests<T> : IListGenericTests<T>
{
    protected override bool EnumeratorCurrentUndefinedOperationThrows => true;

    #region IList<T> Helper Methods

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
        var toCreateFrom = CreateEnumerable(EnumerableType.List, null!, count, 0, 0);
        return new OrderedList<T>(toCreateFrom);
    }

    protected void VerifyList(OrderedList<T> list, OrderedList<T> expectedItems)
    {
        Assert.Equal(expectedItems.Count, list.Count);

        //Only verify the indexer. List should be in a good enough state that we
        //do not have to verify consistency with any other method.
        for (var i = 0; i < list.Count; ++i)
        {
            if (list[i] is null)
                Assert.Null(expectedItems[i]);
            else
                Assert.Equal(list[i], expectedItems[i]);
        }
    }

    #endregion
}