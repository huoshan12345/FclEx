#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
namespace System.Collections.Generic.OrderedIndex;

public abstract class OrderedIndexTests<TScore, TValue> : IGenericSharedApiTests<(TScore, TValue)>
    where TValue : notnull
{
    protected override bool EnumeratorCurrentUndefinedOperationThrows => false;
    protected override bool DefaultValueAllowed => false;
    protected override bool DuplicateValuesAllowed => false;
    protected override bool DefaultValueWhenNotAllowed_Throws => false;

    #region OrderedIndex<TScore, TValue> Helper Methods

    #region IGenericSharedAPI<T> Helper Methods

    protected OrderedIndex<TScore, TValue> Factory() => [];

    protected OrderedIndex<TScore, TValue> Factory(int count)
    {
        var heap = new OrderedIndex<TScore, TValue>();
        var seed = count * 34;
        for (var i = 0; i < count; i++)
            heap.Add(CreateT(seed++));
        return heap;
    }

    #endregion

    protected override IEnumerable<(TScore, TValue)> GenericIEnumerableFactory()
    {
        return Factory();
    }

    protected override IEnumerable<(TScore, TValue)> GenericIEnumerableFactory(int count)
    {
        return Factory(count);
    }

    protected override int Count(IEnumerable<(TScore, TValue)> enumerable)
        => ((OrderedIndex<TScore, TValue>)enumerable).Count;
    protected override void Add(IEnumerable<(TScore, TValue)> enumerable, (TScore, TValue) value)
        => ((OrderedIndex<TScore, TValue>)enumerable).Add(value);
    protected override void Clear(IEnumerable<(TScore, TValue)> enumerable)
        => ((OrderedIndex<TScore, TValue>)enumerable).Clear();
    protected override bool Contains(IEnumerable<(TScore, TValue)> enumerable, (TScore, TValue) value)
        => ((OrderedIndex<TScore, TValue>)enumerable).Contains(value);
    protected override void CopyTo(IEnumerable<(TScore, TValue)> enumerable, (TScore, TValue)[] array, int index)
        => ((OrderedIndex<TScore, TValue>)enumerable).CopyTo(array, index);
    protected override bool Remove(IEnumerable<(TScore, TValue)> enumerable)
    {
        var col = ((ICollection<(TScore, TValue)>)enumerable);
        var item = col.FirstOrDefault();
        return col.Remove(item);
    }

    #endregion

    #region Constructor

    #endregion

    #region Constructor_IEnumerable

    [Theory]
    [MemberData(nameof(EnumerableTestData))]
    public void Generic_Constructor_IEnumerable(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        var arr = CreateEnumerable(enumerableType, null!, enumerableLength, 0, 0).ToArray();

        var comparer = Comparer<TScore>.Default;
        var orderedIndex = new OrderedIndex<TScore, TValue>(comparer);
        foreach (var (score, value) in arr)
        {
            orderedIndex.Add(score, value);
        }

        Assert.Equal(arr.Length, orderedIndex.Count);
        arr.StableSort(Comparer<(TScore, TValue)>.Create((x, y)
            => comparer.Compare(x.Item1, y.Item1)));

        foreach (var (x, y) in arr.Zip(orderedIndex))
        {
            Assert.Equal(x, y);
        }
    }

    #endregion

    #region ToArray

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Generic_ToArray(int count)
    {
        var orderedIndex = Factory(count);
        Assert.True(ArrayExtensions.SequenceEqual(orderedIndex.ToArray(), orderedIndex.ToArray<(TScore, TValue)>()));
    }

    #endregion
}
