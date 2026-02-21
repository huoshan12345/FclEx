namespace System.Collections.Generic.Heap;

public class HeapTestsString : HeapTests<string>
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

public class HeapTestsInt : HeapTests<int>
{
    protected override int CreateT(int seed) => new Random(seed).Next();
}

public class HeapTestsSimpleInt : HeapTests<SimpleInt>
{
    protected override SimpleInt CreateT(int seed) => new(new Random(seed).Next());
}