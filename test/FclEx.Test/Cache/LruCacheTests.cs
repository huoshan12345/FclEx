using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FclEx.Cache;
using FclEx.Utils;
using Xunit;

namespace FclEx.Test.Cache
{
    public class LruCacheTests
    {
        [Fact]
        public void GetOrAdd_Test()
        {
            const int capacity = 10;
            var random = new Random(31);
            var numbers = Enumerable.Range(-8, 16).ToArray();
            var cache = new LruCache<int, string>(capacity);
            var dic = numbers.ToDictionary(m => m, m => default(DateTime?));

            for (var i = 0; i < numbers.Length * capacity; i++)
            {
                var num = numbers.GetRandomly(random);
                var exist = dic.TryGetValue(num, out var existTime) && existTime.HasValue;
                dic[num] = DateTime.UtcNow;

                var keys = cache.GetKeys();
                Assert.Equal(exist, keys.Contains(num));

                var last = cache.LastOrDefault();
                var value = cache.GetOrAdd(num, k => k.ToString());

                Assert.Equal(num.ToString(), value);
                Assert.Equal(num, cache.First().Key);
                Assert.True(cache.Count <= capacity);

                var newKeys = cache.GetKeys();
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
    }
}
