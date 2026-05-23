namespace FclEx.Utils;

public class LruCacheTests
{
    [Fact]
    public void Fuzz_Test()
    {
        const int capacity = 10;
        var random = new Random(31);
        var numbers = Enumerable.Range(-8, 16).ToArray();
        var cache = new LruCache<int, string>(capacity);
        var dic = numbers.ToDictionary(m => m, m => default(int?));
        var access = 0;

        for (var i = 0; i < numbers.Length * capacity; i++)
        {
            var num = random.Sample(numbers);
            var exist = dic.TryGetValue(num, out var existTime) && existTime.HasValue;
            dic[num] = access++;

            var keys = cache.Keys;
            Assert.Equal(exist, keys.Contains(num));

            var last = cache.LastOrDefault();
            var value = cache.GetOrAdd(num, k => k.ToString());

            Assert.Equal(num.ToString(), value);
            Assert.Equal(num, cache.First().Key);
            Assert.True(cache.Count <= capacity);

            var newKeys = cache.Keys;
            if (exist)
            {
                var list = new List<int>(newKeys.Count) { num };
                list.AddRange(keys.Where(m => m != num));
                Assert.Equal(list, newKeys);
            }

            var removeFlag = keys.Count == capacity && !keys.Contains(num);
            if (removeFlag)
            {
                var existItems = dic.Where(m => m.Value.HasValue).ToList();
                Assert.True(cache.Count == capacity);
                Assert.True(existItems.Count == capacity + 1);

                var expectedRemoveItem = existItems.OrderBy(m => m.Value.Get()).First().Key;
                Assert.Equal(expectedRemoveItem, last.Key);

                dic[expectedRemoveItem] = null;
            }
            var expectedCacheItems = dic
                .Where(m => m.Value.HasValue)
                .OrderByDescending(m => m.Value.Get())
                .Select(m => m.Key);
            Assert.Equal(expectedCacheItems, newKeys);
        }
    }

    [Fact]
    public async Task Basic_Test()
    {
        var cd = new LruCache<int, int>(10);

        var tks = new Task[2];
        tks[0] = Task.Run(() =>
        {
            var ret = cd.TryAdd(1, 11);
            if (!ret)
            {
                var value = cd.AddOrUpdate(1, 11);
                Assert.Equal(11, value);
            }

            ret = cd.TryAdd(2, 22);
            if (!ret)
            {
                var value = cd.AddOrUpdate(2, 22);
                Assert.Equal(22, value);
            }
        });

        tks[1] = Task.Run(() =>
        {
            var ret = cd.TryAdd(2, 222);
            if (!ret)
            {
                var value = cd.AddOrUpdate(2, 222);
                Assert.Equal(222, value);
            }

            ret = cd.TryAdd(1, 111);
            if (!ret)
            {
                var value = cd.AddOrUpdate(1, 111);
                Assert.Equal(111, value);
            }
        });

        await Task.WhenAll(tks);
    }

    [Theory]
    [InlineData(1, 1, 1, 1000)]
    [InlineData(5, 1, 1, 1000)]
    [InlineData(1, 1, 2, 500)]
    [InlineData(1, 1, 5, 200)]
    [InlineData(4, 0, 4, 200)]
    [InlineData(16, 31, 4, 200)]
    [InlineData(64, 5, 5, 500)]
    [InlineData(5, 5, 5, 250)]
    public void Add_Test(int cLevel, int initSize, int threads, int addsPerThread)
    {
        var dict = new LruCache<int, int>();

        var count = threads;
        using (var mre = new ManualResetEvent(false))
        {
            for (var i = 0; i < threads; i++)
            {
                var ii = i;
                Task.Run(() =>
                {
                    for (var j = 0; j < addsPerThread; j++)
                    {
                        dict.TryAdd(j + ii * addsPerThread, -(j + ii * addsPerThread));
                    }
                    if (Interlocked.Decrement(ref count) == 0)
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        mre.Set();
                    }
                });
            }
            mre.WaitOne();
        }

        foreach (var pair in dict)
        {
            Assert.Equal(pair.Key, -pair.Value);
        }

        var gotKeys = new List<int>();
        foreach (var pair in dict)
            gotKeys.Add(pair.Key);

        gotKeys.Sort();

        var expectKeys = new List<int>();
        var itemCount = threads * addsPerThread;
        for (var i = 0; i < itemCount; i++)
            expectKeys.Add(i);

        Assert.Equal(expectKeys.Count, gotKeys.Count);

        for (var i = 0; i < expectKeys.Count; i++)
        {
            Assert.True(expectKeys[i].Equals(gotKeys[i]),
                string.Format("The set of keys in the dictionary is are not the same as the expected" + Environment.NewLine +
                              "TestAdd1(cLevel={0}, initSize={1}, threads={2}, addsPerThread={3})", cLevel, initSize, threads, addsPerThread)
            );
        }

        // Finally, let's verify that the count is reported correctly.
        var expectedCount = threads * addsPerThread;
        Assert.Equal(expectedCount, dict.Count);
        Assert.Equal(expectedCount, dict.ToArray().Length);
    }

    [Theory]
    [InlineData(1, 1, 1000)]
    [InlineData(5, 1, 1000)]
    [InlineData(1, 2, 500)]
    [InlineData(1, 5, 200)]
    [InlineData(4, 4, 200)]
    [InlineData(15, 5, 201)]
    [InlineData(64, 5, 500)]
    [InlineData(5, 5, 250)]
    public void Update_Test(int cLevel, int threads, int updatesPerThread)
    {
        var dict = new LruCache<int, int>();

        for (var i = 1; i <= updatesPerThread; i++) dict[i] = i;

        var running = threads;
        using (var mre = new ManualResetEvent(false))
        {
            for (var i = 0; i < threads; i++)
            {
                var ii = i;
                Task.Run(() =>
                {
                    for (var j = 1; j <= updatesPerThread; j++)
                    {
                        dict[j] = (ii + 2) * j;
                    }
                    if (Interlocked.Decrement(ref running) == 0)
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        mre.Set();
                    }
                });
            }
            mre.WaitOne();
        }

        foreach (var pair in dict)
        {
            var div = pair.Value / pair.Key;
            var rem = pair.Value % pair.Key;

            Assert.Equal(0, rem);
            Assert.True(div > 1 && div <= threads + 1,
                string.Format("* Invalid value={3}! TestUpdate1(cLevel={0}, threads={1}, updatesPerThread={2})", cLevel, threads, updatesPerThread, div));
        }

        var gotKeys = new List<int>();
        foreach (var pair in dict)
            gotKeys.Add(pair.Key);
        gotKeys.Sort();

        var expectKeys = new List<int>();
        for (var i = 1; i <= updatesPerThread; i++)
            expectKeys.Add(i);

        Assert.Equal(expectKeys.Count, gotKeys.Count);

        for (var i = 0; i < expectKeys.Count; i++)
        {
            Assert.True(expectKeys[i].Equals(gotKeys[i]),
                string.Format("The set of keys in the dictionary is are not the same as the expected." + Environment.NewLine +
                              "TestUpdate1(cLevel={0}, threads={1}, updatesPerThread={2})", cLevel, threads, updatesPerThread)
            );
        }
    }
}