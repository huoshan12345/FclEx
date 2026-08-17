namespace FclEx.Extensions;

public class ReadOnlyListExtensionsTests
{
    [Fact]
    public void TryGet_ReturnsTrueAndNullForANullItem()
    {
        IReadOnlyList<string?> list = new string?[] { null };

        var found = list.TryGet(0, out var value);

        Assert.True(found);
        Assert.Null(value);
    }
}
