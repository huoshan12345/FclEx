namespace System.Collections.Generic.OrderedList;

public abstract partial class OrderedListTests<T>
{
    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Reverse(int listLength)
    {
        var list = GenericListFactory(listLength);
        var listBefore = list.ToList();

        _ = list.Reverse();

        for (var i = 0; i < listBefore.Count; i++)
        {
            Assert.Equal(list[i], listBefore[^(i + 1)]); //"Expect them to be the same."
        }
    }
}