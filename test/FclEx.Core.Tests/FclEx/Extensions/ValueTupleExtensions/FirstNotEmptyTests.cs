namespace FclEx.Extensions.ValueTupleExtensions;

public class FirstNotEmptyTests
{
    private readonly ITestOutputHelper _output;

    public FirstNotEmptyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TwoElements_Test()
    {
        foreach (var (i, items) in new[] { "test", "test2", string.Empty, null }.ToVariations(2).Index())
        {
            Assert.Equal(2, items.Count);
            var result = (items[0], items[1]).FirstNotEmpty(null);
            Assert.Equal(items.FirstOrDefault(m => m.IsNotEmpty()), result);
        }
    }

    [Fact]
    public void ThreeElements_Test()
    {
        foreach (var (i, items) in new[] { "test", "test2", "test3", string.Empty, null }.ToVariations(3).Index())
        {
            Assert.Equal(3, items.Count);
            var result = (items[0], items[1], items[2]).FirstNotEmpty(null);
            Assert.Equal(items.FirstOrDefault(m => m.IsNotEmpty()), result);
        }
    }

    [Fact]
    public void FourElements_Test()
    {
        foreach (var (i, items) in new[] { "test", "test2", "test3", "test4", string.Empty, null }.ToVariations(4).Index())
        {
            Assert.Equal(4, items.Count);
            var result = (items[0], items[1], items[2], items[3]).FirstNotEmpty(null);
            Assert.Equal(items.FirstOrDefault(m => m.IsNotEmpty()), result);
        }
    }
}