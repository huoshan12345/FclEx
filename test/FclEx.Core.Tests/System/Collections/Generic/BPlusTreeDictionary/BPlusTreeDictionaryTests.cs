namespace System.Collections.Generic.BPlusTreeDictionary;

public abstract class BPlusTreeDictionaryTests<TKey, TValue> : IDictionary_Generic_Tests<TKey, TValue> where TKey : notnull
{
    #region IDictionary<TKey, TValue> Helper Methods
    protected override bool Enumerator_Empty_UsesSingletonInstance => false;
    protected override bool Enumerator_Empty_Current_UndefinedOperation_Throws => true;
    protected override bool Enumerator_Empty_ModifiedDuringEnumeration_ThrowsInvalidOperationException => true; // do not allow modification during enumeration even it was empty
    protected override Type ICollection_Generic_CopyTo_IndexLargerThanArrayCount_ThrowType => typeof(ArgumentOutOfRangeException);
    protected override ModifyOperation ModifyEnumeratorThrows => ModifyOperation.Add | ModifyOperation.Insert | ModifyOperation.Remove | ModifyOperation.Clear;
    protected override ModifyOperation ModifyEnumeratorAllowed => ModifyOperation.Overwrite;

    protected override IDictionary<TKey, TValue> GenericIDictionaryFactory()
    {
        return new BPlusTreeDictionary<TKey, TValue>();
    }

    #endregion

    #region Constructors

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void BPlusTreeDictionary_Generic_Constructor_IComparer(int count)
    {
        var comparer = GetKeyIComparer();
        var source = GenericIDictionaryFactory(count);
        var copied = new BPlusTreeDictionary<TKey, TValue>(source, comparer);
        Assert.Equal(source, copied);
        Assert.Equal(comparer, copied.Comparer);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void BPlusTreeDictionary_Generic_Constructor_IDictionary(int count)
    {
        var source = GenericIDictionaryFactory(count);
        IDictionary<TKey, TValue> copied = new BPlusTreeDictionary<TKey, TValue>(source);
        Assert.Equal(source, copied);
    }

    [Fact]
    public void BPlusTreeDictionary_Generic_Constructor_NullIDictionary_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new BPlusTreeDictionary<TKey, TValue>(null));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void BPlusTreeDictionary_Generic_Constructor_IDictionary_IComparer(int count)
    {
        var comparer = GetKeyIComparer();
        var source = GenericIDictionaryFactory(count);
        var sourceSorted = new BPlusTreeDictionary<TKey, TValue>(source, comparer);
        Assert.Equal(source, sourceSorted);
        Assert.Equal(comparer, sourceSorted.Comparer);
        // Test copying a sorted dictionary.
        var copied = new BPlusTreeDictionary<TKey, TValue>(sourceSorted, comparer);
        Assert.Equal(sourceSorted, copied);
        Assert.Equal(comparer, copied.Comparer);
        // Test copying a sorted dictionary with a different comparer.
        IComparer<TKey> reverseComparer = Comparer<TKey>.Create((key1, key2) => -comparer.Compare(key1, key2));
        var copiedReverse = new BPlusTreeDictionary<TKey, TValue>(sourceSorted, reverseComparer);

        Assert.Equal(sourceSorted.Count, copiedReverse.Count);

        // reverse twice to get back to original order for equality check
        foreach (var (x, y) in sourceSorted.Zip(copiedReverse.Reverse()))
        {
            // NOTE: do not use Assert.Equal(sourceSorted, copiedReverse.Reverse()); 
            // because it has special handling for collections that implement IDictionary in which it will ignore the order.
            // although BPlusTreeDictionary does not implement IDictionary, we still don't want to rely on that special handling for this test.
            Assert.Equal(x, y);
        }

        Assert.Equal(reverseComparer, copiedReverse.Comparer);
    }

    #endregion

    #region ContainsValue

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void BPlusTreeDictionary_Generic_ContainsValue_NotPresent(int count)
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
    public void BPlusTreeDictionary_Generic_ContainsValue_Present(int count)
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
    public void BPlusTreeDictionary_Generic_ContainsValue_DefaultValueNotPresent(int count)
    {
        var dictionary = (BPlusTreeDictionary<TKey, TValue>)GenericIDictionaryFactory(count);
        Assert.False(dictionary.ContainsValue(default!));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void BPlusTreeDictionary_Generic_ContainsValue_DefaultValuePresent(int count)
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

    #region Ordering

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void BPlusTreeDictionary_Generic_DictionaryIsProperlySortedAccordingToComparer(int setLength)
    {
        var set = (BPlusTreeDictionary<TKey, TValue>)GenericIDictionaryFactory(setLength);
        var expected = set.ToList();
        expected.Sort(GetIComparer());
        var expectedIndex = 0;
        foreach (var value in set)
            Assert.Equal(expected[expectedIndex++], value);
    }

    #endregion

    #region IReadOnlyDictionary<TKey, TValue>.Keys

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IReadOnlyDictionary_Generic_Keys_ContainsAllCorrectKeys(int count)
    {
        var dictionary = GenericIDictionaryFactory(count);
        var expected = dictionary.Select(pair => pair.Key);
        var keys = ((IReadOnlyDictionary<TKey, TValue>)dictionary).Keys;
        Assert.True(expected.SequenceEqual(keys));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IReadOnlyDictionary_Generic_Values_ContainsAllCorrectValues(int count)
    {
        var dictionary = GenericIDictionaryFactory(count);
        var expected = dictionary.Select(pair => pair.Value);
        var values = ((IReadOnlyDictionary<TKey, TValue>)dictionary).Values;
        Assert.True(expected.SequenceEqual(values));
    }

    #endregion
}