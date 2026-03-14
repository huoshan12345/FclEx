namespace System.Collections.Generic.OrderedList;

public class OrderedListTestsString : OrderedListTests<string>
{
    protected override string CreateT(int seed)
    {
        var stringLength = seed % 10 + 5;
        var rand = new Random(seed);
        var bytes = new byte[stringLength];
        rand.NextBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}

public class OrderedListTestsInt : OrderedListTests<int>
{
    protected override int CreateT(int seed)
    {
        var rand = new Random(seed);
        return rand.Next();
    }
}

public class OrderedListTestsSimpleInt : OrderedListTests<SimpleInt>
{
    protected override SimpleInt CreateT(int seed)
    {
        var rand = new Random(seed);
        return new(rand.Next());
    }
}