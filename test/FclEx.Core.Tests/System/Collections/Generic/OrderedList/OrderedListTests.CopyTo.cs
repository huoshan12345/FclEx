// ReSharper disable InvokeAsExtensionMember
namespace System.Collections.Generic.OrderedList;

/// <summary>
/// Contains tests that ensure the correctness of the List class.
/// </summary>
public abstract partial class OrderedListTests<T>
{
    [Fact]
    public void CopyTo_InvalidArgs_Throws()
    {
        AssertExtensions.Throws<ArgumentNullException>("list", () => CollectionExtensions.CopyTo(null!, Span<int>.Empty));
        AssertExtensions.Throws<ArgumentNullException>("list", () => CollectionExtensions.CopyTo(null!, new Span<int>(new int[1])));

        var list = new List<int>() { 1, 2, 3 };
        Assert.Throws<ArgumentException>(() => CollectionExtensions.CopyTo(list, (Span<int>)new int[2]));
    }

    [Fact]
    public void CopyTo_ItemsCopiedCorrectly()
    {
        List<int> list = [];
        var destination = Span<int>.Empty;
        list.CopyTo(destination);

        list = new List<int>() { 1, 2, 3 };
        destination = new int[3];
        list.CopyTo(destination);
        Assert.Equal(new[] { 1, 2, 3 }, destination.ToArray());

        list = new List<int>() { 1, 2, 3 };
        destination = new int[4];
        list.CopyTo(destination);
        Assert.Equal(new[] { 1, 2, 3, 0 }, destination.ToArray());
    }
}