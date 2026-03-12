#pragma warning disable CA1859 // Use concrete types when possible for improved performance
namespace System.Collections.Generic;

public abstract class BPlusTreeDictionaryTests<TKey, TValue> : IDictionaryGenericTests<TKey, TValue> where TKey : notnull
{
    #region IDictionary<TKey, TValue Helper Methods

    protected override bool EnumeratorCurrentUndefinedOperationThrows => false;

    protected override IDictionary<TKey, TValue> GenericIDictionaryFactory() => new BPlusTreeDictionary<TKey, TValue>();

    #endregion

    #region Constructors

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Generic_Constructor_IDictionary(int count)
    {
        var source = GenericIDictionaryFactory(count);
        IDictionary<TKey, TValue> copied = new BPlusTreeDictionary<TKey, TValue>(source);
        Assert.Equal(source.Count, copied.Count);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Generic_Constructor_IDictionary_IComparer(int count)
    {
        var comparer = GetKeyIComparer();
        var source = GenericIDictionaryFactory(count);
        var copied = new BPlusTreeDictionary<TKey, TValue>(source, comparer);
        Assert.Equal(source, copied);
    }


    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Generic_Constructor_IComparer(int count)
    {
        var comparer = GetKeyIComparer();
        var source = GenericIDictionaryFactory(count);
        var copied = new BPlusTreeDictionary<TKey, TValue>(source, comparer);
        Assert.Equal(source, copied);
    }

    #endregion

    #region Ordering

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void SortedDictionary_Generic_DictionaryIsProperlySortedAccordingToComparer(int setLength)
    {
        var set = GenericIDictionaryFactory(setLength);
        var expected = set.ToList();
        expected.Sort(GetIComparer());
        var expectedIndex = 0;
        foreach (var value in set)
            Assert.Equal(expected[expectedIndex++], value);
    }

    #endregion

    #region ContainsValue

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Generic_ContainsValue_NotPresent(int count)
    {
        var dictionary = (BPlusTreeDictionary<TKey, TValue>)GenericIDictionaryFactory(count);
        var seed = 4315;
        var notPresent = CreateTValue(seed++);
        while (dictionary.Values.Contains(notPresent))
            notPresent = CreateTValue(seed++);
        Assert.False(dictionary.ContainsValue(notPresent));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Generic_ContainsValue_Present(int count)
    {
        var dictionary = (BPlusTreeDictionary<TKey, TValue>)GenericIDictionaryFactory(count);
        var seed = 4315;
        var notPresent = CreateT(seed++);
        while (dictionary.Contains(notPresent))
            notPresent = CreateT(seed++);
        dictionary.Add(notPresent.Key, notPresent.Value);
        Assert.True(dictionary.ContainsValue(notPresent.Value));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Generic_ContainsValue_DefaultValueNotPresent(int count)
    {
        var dictionary = (BPlusTreeDictionary<TKey, TValue>)GenericIDictionaryFactory(count);
        Assert.False(dictionary.ContainsValue(default!));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Generic_ContainsValue_DefaultValuePresent(int count)
    {
        var dictionary = (BPlusTreeDictionary<TKey, TValue>)GenericIDictionaryFactory(count);
        var seed = 4315;
        var notPresent = CreateTKey(seed++);
        while (dictionary.ContainsKey(notPresent))
            notPresent = CreateTKey(seed++);
        dictionary.Add(notPresent, default!);
        Assert.True(dictionary.ContainsValue(default!));
    }

    #endregion
}