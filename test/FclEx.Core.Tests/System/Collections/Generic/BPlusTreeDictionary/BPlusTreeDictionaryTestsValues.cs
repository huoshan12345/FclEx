namespace System.Collections.Generic.BPlusTreeDictionary;

public class BPlusTreeDictionaryTestsValues : ICollection_Generic_Tests<string>
{
    protected override bool Enumerator_Empty_UsesSingletonInstance => true;
    protected override bool Enumerator_Empty_Current_UndefinedOperation_Throws => true;
    protected override bool DefaultValueAllowed => false;
    protected override bool DuplicateValuesAllowed => false;
    protected override bool IsReadOnly => true;
    protected override IEnumerable<ModifyEnumerable> GetModifyEnumerables(ModifyOperation operations) => new List<ModifyEnumerable>();

    protected override ICollection<string> GenericICollectionFactory() => new BPlusTreeDictionary<string, string>().Values;

    protected override ICollection<string> GenericICollectionFactory(int count)
    {
        var list = new BPlusTreeDictionary<string, string>();
        var seed = 13453;
        for (var i = 0; i < count; i++)
            list.Add(CreateT(seed++), CreateT(seed++));
        return list.Values;
    }

    protected override string CreateT(int seed)
    {
        var stringLength = seed % 10 + 5;
        var rand = new Random(seed);
        var bytes = new byte[stringLength];
        rand.NextBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    protected override Type ICollection_Generic_CopyTo_IndexLargerThanArrayCount_ThrowType => typeof(ArgumentOutOfRangeException);

    [Fact]
    public void BPlusTreeDictionary_Generic_ValueCollection_Constructor_NullDictionary()
    {
        Assert.Throws<ArgumentNullException>(() => new BPlusTreeDictionary<string, string>.ValueCollection(null!));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void BPlusTreeDictionary_Generic_ValueCollection_GetEnumerator(int count)
    {
        var dictionary = new BPlusTreeDictionary<string, string>();
        var seed = 13453;
        while (dictionary.Count < count)
            dictionary.Add(CreateT(seed++), CreateT(seed++));
        using var _ = dictionary.Values.GetEnumerator();
    }
}