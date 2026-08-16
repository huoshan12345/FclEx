namespace FclEx.Utils;

public class LruCacheTests
{
    [Fact]
    public void LeastRecentlyUsedEntry_IsEvicted()
    {
        var cache = new LruCache<int, string>(2);
        cache.Set(1, "one");
        cache.Set(2, "two");

        Assert.True(cache.TryGetValue(1, out _));
        cache.Set(3, "three");

        Assert.Equal(new[] { 3, 1 }, cache.Keys);
        Assert.False(cache.TryGetValue(2, out _));
    }

    [Fact]
    public async Task GetOrAdd_ConcurrentCallsForSameKey_InvokeFactoryOnce()
    {
        var cache = new LruCache<int, object>(2);
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
    public void GetOrAdd_FactoryRunsOutsideLockAndCanUseAnotherKey()
    {
        var cache = new LruCache<int, int>(2);

        var value = cache.GetOrAdd(1, _ => cache.GetOrAdd(2, _ => 20) + 1);

        Assert.Equal(21, value);
        Assert.Equal(2, cache.Count);
    }

    [Fact]
    public void FailedFactory_CanBeRetried()
    {
        var cache = new LruCache<int, int>(1);
        var calls = 0;

        Assert.Throws<InvalidOperationException>(() => cache.GetOrAdd(1, _ =>
        {
            calls++;
            throw new InvalidOperationException();
        }));

        Assert.Equal(42, cache.GetOrAdd(1, _ =>
        {
            calls++;
            return 42;
        }));
        Assert.Equal(2, calls);
    }

    [Fact]
    public void EntryRemoved_ReportsEveryRemovalReasonOutsideLock()
    {
        var cache = new LruCache<int, string>(2);
        var notifications = new List<(int Key, string Value, CacheEntryRemovalReason Reason)>();
        cache.EntryRemoved += (_, args) =>
        {
            _ = cache.Count;
            notifications.Add((args.Key, args.Value, args.Reason));
        };

        cache.Set(1, "one");
        cache.Set(1, "ONE");
        Assert.True(cache.Remove(1));
        cache.Set(2, "two");
        cache.Set(3, "three");
        cache.Set(4, "four");
        cache.Clear();

        Assert.Contains((1, "one", CacheEntryRemovalReason.Replaced), notifications);
        Assert.Contains((1, "ONE", CacheEntryRemovalReason.Removed), notifications);
        Assert.Contains((2, "two", CacheEntryRemovalReason.Evicted), notifications);
        Assert.Contains((3, "three", CacheEntryRemovalReason.Cleared), notifications);
        Assert.Contains((4, "four", CacheEntryRemovalReason.Cleared), notifications);
        Assert.Equal(5, notifications.Count);
    }

    [Fact]
    public void ThrowingRemovalHandlers_DoNotInterruptStateChangeOrOtherHandlers()
    {
        var cache = new LruCache<int, int>(1);
        cache.Set(1, 1);
        var handlerCalls = 0;
        cache.EntryRemoved += (_, _) =>
        {
            handlerCalls++;
            throw new InvalidOperationException("first");
        };
        cache.EntryRemoved += (_, _) =>
        {
            handlerCalls++;
            throw new ArgumentException("second");
        };

        var exception = Assert.Throws<AggregateException>(() => cache.Set(2, 2));

        Assert.Equal(2, handlerCalls);
        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.Equal(2, cache[2]);
        Assert.False(cache.TryGetValue(1, out _));
    }

    [Fact]
    public async Task ConcurrentAccess_PreservesCapacityAndUniqueKeys()
    {
        const int capacity = 32;
        var cache = new LruCache<int, int>(capacity);
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
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RejectsNonPositiveCapacity(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LruCache<int, int>(capacity));
    }
}
