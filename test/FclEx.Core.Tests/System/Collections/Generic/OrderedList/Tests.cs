#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
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
        return new OrderedList<T>();
    }

    protected virtual OrderedList<T> GenericListFactory(int count)
    {
        var toCreateFrom = CreateEnumerable(EnumerableType.List, null, count, 0, 0);
        return new OrderedList<T>(toCreateFrom);
    }

    protected void VerifyList(OrderedList<T> list, OrderedList<T> expectedItems)
    {
        Assert.Equal(expectedItems.Count, list.Count);

        //Only verify the indexer. List should be in a good enough state that we
        //do not have to verify consistency with any other method.
        for (var i = 0; i < list.Count; ++i)
        {
            Assert.True(list[i] == null ? expectedItems[i] == null : list[i].Equals(expectedItems[i]));
        }
    }

    #endregion
}