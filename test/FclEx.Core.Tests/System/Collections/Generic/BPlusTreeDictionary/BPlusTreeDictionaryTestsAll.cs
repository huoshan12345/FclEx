namespace System.Collections.Generic.BPlusTreeDictionary;

public class BPlusTreeDictionaryTests_String_String : BPlusTreeDictionaryTests<string, string>
{
    protected override KeyValuePair<string, string> CreateT(int seed)
    {
        return new KeyValuePair<string, string>(CreateTKey(seed), CreateTKey(seed + 500));
    }

    protected override string CreateTKey(int seed)
    {
        var stringLength = seed % 10 + 5;
        var rand = new Random(seed);
        var bytes1 = new byte[stringLength];
        rand.NextBytes(bytes1);
        return Convert.ToBase64String(bytes1);
    }

    protected override string CreateTValue(int seed)
    {
        return CreateTKey(seed);
    }
}

public class BPlusTreeDictionaryTests_Int_Int : BPlusTreeDictionaryTests<int, int>
{
    protected override bool DefaultValueAllowed => true;

    protected override KeyValuePair<int, int> CreateT(int seed)
    {
        var rand = new Random(seed);
        return new KeyValuePair<int, int>(rand.Next(), rand.Next());
    }

    protected override int CreateTKey(int seed)
    {
        var rand = new Random(seed);
        return rand.Next();
    }

    protected override int CreateTValue(int seed)
    {
        return CreateTKey(seed);
    }
}

public class BPlusTreeDictionaryTests_EquatableBackwardsOrder_Int : BPlusTreeDictionaryTests<EquatableBackwardsOrder, int>
{
    protected override KeyValuePair<EquatableBackwardsOrder, int> CreateT(int seed)
    {
        var rand = new Random(seed);
        return new KeyValuePair<EquatableBackwardsOrder, int>(new EquatableBackwardsOrder(rand.Next()), rand.Next());
    }

    protected override EquatableBackwardsOrder CreateTKey(int seed)
    {
        var rand = new Random(seed);
        return new EquatableBackwardsOrder(rand.Next());
    }

    protected override int CreateTValue(int seed)
    {
        var rand = new Random(seed);
        return rand.Next();
    }

    protected override IDictionary<EquatableBackwardsOrder, int> GenericIDictionaryFactory()
    {
        return new BPlusTreeDictionary<EquatableBackwardsOrder, int>();
    }
}