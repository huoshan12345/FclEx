namespace System.Collections.Generic.OrderedIndex;

public class OrderedIndexTests_String_String : OrderedIndexTests<string, string>
{
    protected override (string, string) CreateT(int seed)
    {
        var l = Random.Shared.NextString(5);
        var r = Random.Shared.NextString(10);
        return (l, r);
    }
}

public class OrderedIndexTests_Int_Int : OrderedIndexTests<int, int>
{
    protected override (int, int) CreateT(int seed)
    {
        var l = Random.Shared.Next();
        var r = Random.Shared.Next();
        return (l, r);
    }
}

public class OrderedIndexTests_SimpleInt_Int : OrderedIndexTests<SimpleInt, int>
{
    protected override (SimpleInt, int) CreateT(int seed)
    {
        var l = Random.Shared.Next();
        var r = Random.Shared.Next();
        return (new(l), r);
    }
}