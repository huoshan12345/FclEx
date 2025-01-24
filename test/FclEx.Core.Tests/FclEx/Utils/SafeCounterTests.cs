namespace FclEx.Utils;

public class SafeCounterTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void Increment_Test(int seed)
    {
        var counter = new SafeCounter(seed);
        Assert.Equal(seed + 1, counter.Increment());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void Decrement_Test(int seed)
    {
        var counter = new SafeCounter(seed);
        Assert.Equal(seed - 1, counter.Decrement());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void Add_Test(int num)
    {
        const int seed = 2;
        var counter = new SafeCounter(seed);
        Assert.Equal(seed + num, counter.Add(num));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void Set_Test(int num)
    {
        var counter = new SafeCounter(2);
        var value = counter.Value;
        Assert.Equal(value, counter.Set(num));
        Assert.Equal(num, counter.Value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void Reset_Test(int seed)
    {
        var counter = new SafeCounter(seed);
        var value = counter.Value;
        Assert.Equal(value, counter.Reset());
        Assert.Equal(0, counter.Value);
    }
}