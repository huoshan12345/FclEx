namespace System.Collections.Generic.Heap;

public class HeapTests_String : HeapTests<string>
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

public class HeapTests_Int : HeapTests<int>
{
    protected override int CreateT(int seed) => new Random(seed).Next();
}

public class HeapTests_String_CustomComparer : HeapTests_String
{
    protected override IComparer<string> GetComparer() => StringComparer.InvariantCultureIgnoreCase;
}

public class HeapTests_Int_CustomComparer : HeapTests_Int
{
    protected override IComparer<int> GetComparer() => Comparer<int>.Create((x, y) => -x.CompareTo(y));
}