using System;
using System.Linq;
using FclEx.Cache;
using MoreLinq;
using Xunit;

namespace FclEx.Test.Cache
{
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
                var num = numbers.GetRandomly(random);
                dic[num] = dic[num].Get(-1) + 1;
                var keys = cache.GetKeys();
                var removeFlag = cache.IsFull() && !keys.Contains(num);
                var value = cache.GetOrAdd(num, k => k.ToString());
                Assert.Equal(num.ToString(), value);
                var newKeys = cache.GetKeys();

                Assert.True(cache.Count <= capacity);

                var expectedCachedItem = dic.Where(m => m.Value.HasValue).ToArray();
                if (removeFlag)
                {
                    Assert.True(cache.Count == capacity);
                    Assert.True(expectedCachedItem.Length == capacity + 1);
                    var expectedRemoveItems = expectedCachedItem.Where(m => m.Key != num)
                        .MinBy(m => m.Value).Select(m => m.Key).ToArray();

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
    }
}
