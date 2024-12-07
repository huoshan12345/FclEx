using MoreLinq;

namespace FclEx.Caches;

public class LfuCacheTests
{
    [Fact]
    public void Test()
    {
        const int capacity = 10;
        var random = new Random(31);
        var numbers = Enumerable.Range(-8, 16).ToArray();
        var cache = new LfuCache<int, string>(capacity);
        var dic = numbers.ToDictionary(m => m, k => default(int?));

        for (var i = 0; i < numbers.Length * capacity; i++)
        {
            var num = random.NextElement(numbers);
            dic[num] = dic[num].Get(-1) + 1;
            var keys = cache.Keys;
            var removeFlag = cache.IsFull() && !keys.Contains(num);
            var value = cache.GetOrAdd(num, k => k.ToString());
            Assert.Equal(num.ToString(), value);
            var newKeys = cache.Keys;

            Assert.True(cache.Count <= capacity);

            var expectedCachedItem = dic.Where(m => m.Value.HasValue).ToArray();
            if (removeFlag)
            {
                Assert.True(cache.Count == capacity);
                Assert.True(expectedCachedItem.Length == capacity + 1);
                var expectedRemoveItems = expectedCachedItem.Where(m => m.Key != num)
                    .Minima(m => m.Value).Select(m => m.Key).ToArray();

                var removedKey = keys.Except(newKeys).Single();
                Assert.Contains(removedKey, expectedRemoveItems);

                var addedKeys = newKeys.Except(keys).ToArray();
                Assert.Single(addedKeys);
                Assert.Equal(num, addedKeys[0]);

                dic[removedKey] = null;
            }
            else
            {
                var actualKeys = newKeys.OrderBy(m => m).ToArray();
                var expectKeys = expectedCachedItem
                    .Where(m => m.Value.HasValue)
                    .OrderByDescending(m => m.Value)
                    .Take(capacity)
                    .Select(m => m.Key)
                    .OrderBy(m => m)
                    .ToArray();
                Assert.True(expectKeys.SequenceEqual(actualKeys));
            }
        }
    }

    [Fact]
    public async Task BasicScenarios_Test()
    {
        var cd = new LfuCache<int, int>(10);

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
}