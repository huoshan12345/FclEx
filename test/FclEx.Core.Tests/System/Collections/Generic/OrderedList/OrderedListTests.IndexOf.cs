namespace System.Collections.Generic.OrderedList;

public abstract partial class OrderedListTests<T>
{
    #region Helpers

    public delegate int IndexOfDelegate(OrderedList<T> list, T value);

    public enum IndexOfMethod
    {
        IndexOf_T,
        IndexOf_T_Int,
        IndexOf_T_Int_Int,
        LastIndexOf_T,
        LastIndexOf_T_Int,
        LastIndexOf_T_Int_Int,
    };

    private static IndexOfDelegate IndexOfDelegateFromType(IndexOfMethod methodType)
    {
        return methodType switch
        {
            IndexOfMethod.IndexOf_T => (list, value) => list.IndexOf(value),
            IndexOfMethod.IndexOf_T_Int => (list, value) => list.IndexOf(value, 0),
            IndexOfMethod.IndexOf_T_Int_Int => (list, value) => list.IndexOf(value, 0, list.Count),
            IndexOfMethod.LastIndexOf_T => (list, value) => list.LastIndexOf(value),
            IndexOfMethod.LastIndexOf_T_Int => (list, value) => list.LastIndexOf(value, list.Count - 1),
            IndexOfMethod.LastIndexOf_T_Int_Int => (list, value) => list.LastIndexOf(value, list.Count - 1, list.Count),
            _ => throw new Exception("Invalid IndexOfMethod")
        };
    }

    /// <summary>
    /// MemberData for a Theory to test the IndexOf methods for List. To avoid high code reuse of tests for the 6 IndexOf
    /// methods in List, delegates are used to cover the basic behavioral cases shared by all IndexOf methods. A bool
    /// is used to specify the ordering (front-to-back or back-to-front (e.g. LastIndexOf)) that the IndexOf method
    /// searches in.
    /// </summary>
    public static IEnumerable<object[]> IndexOfTestData()
    {
        foreach (object[] sizes in ValidCollectionSizes())
        {
            var count = (int)sizes[0];
            yield return [IndexOfMethod.IndexOf_T, count, true];
            yield return [IndexOfMethod.LastIndexOf_T, count, false];

            if (count <= 0)
                continue; // 0 is an invalid index for IndexOf when the count is 0.

            yield return [IndexOfMethod.IndexOf_T_Int, count, true];
            yield return [IndexOfMethod.LastIndexOf_T_Int, count, false];
            yield return [IndexOfMethod.IndexOf_T_Int_Int, count, true];
            yield return [IndexOfMethod.LastIndexOf_T_Int_Int, count, false];
        }
    }

    #endregion

    #region IndexOf

    [Theory]
    [MemberData(nameof(IndexOfTestData))]
    public void IndexOf_NoDuplicates(IndexOfMethod indexOfMethod, int count, bool frontToBackOrder)
    {
        _ = frontToBackOrder;
        var list = GenericListFactory(count);
        var expectedList = list.ToList();
        var indexOf = IndexOfDelegateFromType(indexOfMethod);

        Assert.All(Enumerable.Range(0, count), i =>
        {
            Assert.Equal(i, indexOf(list, expectedList[i]));
        });
    }

    [Theory]
    [MemberData(nameof(IndexOfTestData))]
    public void IndexOf_NonExistingValues(IndexOfMethod indexOfMethod, int count, bool frontToBackOrder)
    {
        _ = frontToBackOrder;
        var list = GenericListFactory(count);
        var nonexistentValues = CreateEnumerable(EnumerableType.List, list, count: count, numberOfMatchingElements: 0, numberOfDuplicateElements: 0);
        var indexOf = IndexOfDelegateFromType(indexOfMethod);

        Assert.All(nonexistentValues, nonexistentValue =>
        {
            Assert.Equal(-1, indexOf(list, nonexistentValue));
        });
    }

    [Theory]
    [MemberData(nameof(IndexOfTestData))]
    public void IndexOf_DefaultValue(IndexOfMethod indexOfMethod, int count, bool frontToBackOrder)
    {
        _ = frontToBackOrder;
        T defaultValue = default!;
        var list = GenericListFactory(count);
        var indexOf = IndexOfDelegateFromType(indexOfMethod);
        while (((ICollection<T>)list).Remove(defaultValue))
            count--;
        list.Add(defaultValue);
        Assert.Equal(count, indexOf(list, defaultValue));
    }

    [Theory]
    [MemberData(nameof(IndexOfTestData))]
    public void IndexOf_OrderIsCorrect(IndexOfMethod indexOfMethod, int count, bool frontToBackOrder)
    {
        var list = GenericListFactory(count);
        var withoutDuplicates = list.ToList();
        list.AddRange(list);
        var indexOf = IndexOfDelegateFromType(indexOfMethod);

        Assert.All(Enumerable.Range(0, count), i =>
        {
            if (frontToBackOrder)
                Assert.Equal(i, indexOf(list, withoutDuplicates[i]));
            else
                Assert.Equal(count + i, indexOf(list, withoutDuplicates[i]));
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IndexOf_Int_OrderIsCorrectWithManyDuplicates(int count)
    {
        var list = GenericListFactory(count);
        var withoutDuplicates = list.ToList();
        list.AddRange(list);
        list.AddRange(list);
        list.AddRange(list);

        Assert.All(Enumerable.Range(0, count), i =>
        {
            Assert.All(Enumerable.Range(0, 4), j =>
            {
                var expectedIndex = j * count + i;
                Assert.Equal(expectedIndex, list.IndexOf(withoutDuplicates[i], count * j));
                Assert.Equal(expectedIndex, list.IndexOf(withoutDuplicates[i], count * j, count));
            });
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void LastIndexOf_Int_OrderIsCorrectWithManyDuplicates(int count)
    {
        var list = GenericListFactory(count);
        var withoutDuplicates = list.ToList();
        list.AddRange(list);
        list.AddRange(list);
        list.AddRange(list);

        Assert.All(Enumerable.Range(0, count), i =>
        {
            Assert.All(Enumerable.Range(0, 4), j =>
            {
                var expectedIndex = j * count + i;
                Assert.Equal(expectedIndex, list.LastIndexOf(withoutDuplicates[i], count * (j + 1) - 1));
                Assert.Equal(expectedIndex, list.LastIndexOf(withoutDuplicates[i], count * (j + 1) - 1, count));
            });
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IndexOf_Int_OutOfRangeExceptions(int count)
    {
        var list = GenericListFactory(count);
        var element = CreateT(234);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count + 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count + 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, int.MinValue));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IndexOf_Int_Int_OutOfRangeExceptions(int count)
    {
        var list = GenericListFactory(count);
        var element = CreateT(234);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count + 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, 0, count + 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count / 2, count / 2 + 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, 0, count + 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, -1, 1));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void LastIndexOf_Int_OutOfRangeExceptions(int count)
    {
        var list = GenericListFactory(count);
        var element = CreateT(234);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, count));
        if (count == 0)  // IndexOf with a 0 count List is special cased to return -1.
            Assert.Equal(-1, list.LastIndexOf(element, -1));
        else
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, -1));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void LastIndexOf_Int_Int_OutOfRangeExceptions(int count)
    {
        var list = GenericListFactory(count);
        var element = CreateT(234);

        if (count > 0)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, 0, count + 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, count / 2, count / 2 + 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, 0, count + 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, -1, count));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, count, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, count, 1));
        }
        else // IndexOf with a 0 count List is special cased to return -1.
        {
            Assert.Equal(-1, list.LastIndexOf(element, 0, count + 1));
            Assert.Equal(-1, list.LastIndexOf(element, count / 2, count / 2 + 2));
            Assert.Equal(-1, list.LastIndexOf(element, 0, count + 1));
            Assert.Equal(-1, list.LastIndexOf(element, 0, -1));
            Assert.Equal(-1, list.LastIndexOf(element, -1, count));
            Assert.Equal(-1, list.LastIndexOf(element, -1, 1));
            Assert.Equal(-1, list.LastIndexOf(element, count, 0));
            Assert.Equal(-1, list.LastIndexOf(element, count, 1));
        }
    }

    #endregion
}