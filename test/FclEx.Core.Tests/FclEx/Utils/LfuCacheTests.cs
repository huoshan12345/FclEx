namespace FclEx.Utils;

public class LfuCacheTests
{
    [Fact]
    public void LeastFrequentlyUsedEntry_IsEvicted()
    {
        var cache = new LfuCache<int, string>(2);
        cache.Set(1, "one");
        cache.Set(2, "two");
        Assert.True(cache.TryGetValue(1, out _));

        cache.Set(3, "three");

        Assert.True(cache.TryGetValue(1, out _));
        Assert.False(cache.TryGetValue(2, out _));
        Assert.True(cache.TryGetValue(3, out _));
    }

    [Fact]
    public void EqualFrequency_UsesLeastRecentlyUsedTieBreaker()
    {
        var cache = new LfuCache<int, string>(2);
        cache.Set(1, "one");
        cache.Set(2, "two");

        cache.Set(3, "three");

        Assert.False(cache.TryGetValue(1, out _));
        Assert.True(cache.TryGetValue(2, out _));
        Assert.True(cache.TryGetValue(3, out _));
    }

    [Fact]
    public void FrequencyDecay_AllowsOldHotEntryToAgeOut()
    {
        var cache = new LfuCache<string, int>(2, frequencyDecayInterval: 4);
        cache.Set("old-hot", 1);
        cache.Set("current-hot", 2);
        for (var i = 0; i < 100; i++)
            Assert.True(cache.TryGetValue("old-hot", out _));
        for (var i = 0; i < 40; i++)
            Assert.True(cache.TryGetValue("current-hot", out _));

        cache.Set("new", 3);

        Assert.False(cache.TryGetValue("old-hot", out _));
        Assert.True(cache.TryGetValue("current-hot", out _));
        Assert.True(cache.TryGetValue("new", out _));
    }

    [Fact]
    public async Task GetOrAdd_ConcurrentCallsForSameKey_InvokeFactoryOnce()
    {
        var cache = new LfuCache<int, object>(2);
        var start = new ManualResetEventSlim();
        var factoryCalls = 0;
        var tasks = Enumerable.Range(0, 16)
            .Select(_ => Task.Run(() =>
            {
                start.Wait();
                return cache.GetOrAdd(1, _ =>
                {
                    Interlocked.Increment(ref factoryCalls);
                    Thread.Sleep(20);
                    return new object();
                });
            }))
            .ToArray();

        start.Set();
        var values = await Task.WhenAll(tasks);

        Assert.Equal(1, factoryCalls);
        Assert.All(values, value => Assert.Same(values[0], value));
    }

    [Fact]
    public void EntryRemoved_HandlerCanReenterCache()
    {
        var cache = new LfuCache<int, int>(1);
        CacheEntryRemovedEventArgs<int, int>? notification = null;
        cache.Set(1, 1);
        cache.EntryRemoved += (_, args) =>
        {
            notification = args;
            Assert.True(cache.TryGetValue(2, out var value));
            Assert.Equal(2, value);
        };

        cache.Set(2, 2);

        Assert.NotNull(notification);
        Assert.Equal(1, notification.Key);
        Assert.Equal(CacheEntryRemovalReason.Evicted, notification.Reason);
    }

    [Fact]
    public async Task ConcurrentAccess_PreservesCapacityAndUniqueKeys()
    {
        const int capacity = 32;
        var cache = new LfuCache<int, int>(capacity, frequencyDecayInterval: 128);
        var tasks = Enumerable.Range(0, 8).Select(worker => Task.Run(() =>
        {
            for (var i = 0; i < 5_000; i++)
            {
                var key = (i + worker) % capacity;
                cache.Set(key, key);
                Assert.True(cache.TryGetValue(key, out var value));
                Assert.Equal(key, value);
            }
        }));

        await Task.WhenAll(tasks);

        Assert.Equal(capacity, cache.Count);
        Assert.Equal(cache.Count, cache.Select(pair => pair.Key).Distinct().Count());
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void Constructor_RejectsInvalidArguments(int capacity, int decayInterval)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LfuCache<int, int>(capacity, decayInterval));
    }
}
