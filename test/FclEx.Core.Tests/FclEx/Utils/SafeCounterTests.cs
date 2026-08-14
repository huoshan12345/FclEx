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

    [Fact]
    public void IncrementAndResetIfThresholdReached_ConcurrentCalls_ClaimsEachBatchOnce()
    {
        const int threshold = 17;
        const int incrementCount = threshold * 100 + 3;
        var counter = new SafeCounter();
        var completedBatches = 0;

        Parallel.For(0, incrementCount, _ =>
        {
            if (counter.IncrementAndResetIfThresholdReached(threshold))
                Interlocked.Increment(ref completedBatches);
        });

        Assert.Equal(100, completedBatches);
        Assert.Equal(3, counter.Value);
    }

    [Fact]
    public async Task IncrementAndInvokeAtThresholdAsync_ConcurrentCalls_InvokeOncePerBatch()
    {
        const int threshold = 10;
        const int incrementCount = threshold * 50;
        var counter = new SafeCounter();
        var actionCalls = 0;

        var tasks = Enumerable.Range(0, incrementCount)
            .Select(_ => Task.Run(() => counter.IncrementAndInvokeAtThresholdAsync(threshold, async () =>
            {
                await Task.Yield();
                Interlocked.Increment(ref actionCalls);
            })))
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.Equal(50, actionCalls);
        Assert.Equal(0, counter.Value);
    }

    [Fact]
    public void IncrementAndInvokeAtThreshold_DoesNotEraseIncrementFromCallback()
    {
        var counter = new SafeCounter(2);

        counter.IncrementAndInvokeAtThreshold(3, () => counter.Increment());

        Assert.Equal(1, counter.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IncrementAndResetIfThresholdReached_RejectsNonPositiveThreshold(int threshold)
    {
        var counter = new SafeCounter();

        Assert.Throws<ArgumentOutOfRangeException>(() => counter.IncrementAndResetIfThresholdReached(threshold));
    }
}
