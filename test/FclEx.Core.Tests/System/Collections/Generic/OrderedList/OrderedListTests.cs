// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Collections.Generic.OrderedList;

/// <summary>
/// Contains tests that ensure the correctness of the List class.
/// </summary>
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

    protected void VerifyList(List<T> list, List<T> expectedItems)
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

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void CopyTo_ArgumentValidity(int count)
    {
        List<T> list = GenericListFactory(count);
        AssertExtensions.Throws<ArgumentException>(null, () => list.CopyTo(0, [], 0, count + 1));
        AssertExtensions.Throws<ArgumentException>(null, () => list.CopyTo(count, [], 0, 1));
    }
}