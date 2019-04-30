using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FclEx.Cache;
using FclEx.Extensions;
using MoreLinq;
using Xunit;

namespace FclEx.Test
{
    public class CounterCacheTests
    {
        [Fact]
        public void Test()
        {
            const int capacity = 10;
            var random = new Random(31);
            var numbers = Enumerable.Range(-8, 16).ToArray();
            var cache = new CounterCache<int, string>(capacity);
            var dic = numbers.ToDictionary(m => m, k => 0);

            for (var i = 0; i < numbers.Length * capacity; i++)
            {
                var num = numbers.Random(random);
                ++dic[num];
                var keys = cache.GetAllKeys();
                var removeFlag = cache.IsFull() && !keys.Contains(num);
                cache.GetOrAdd(num, k => k.ToString());
                var newKeys = cache.GetAllKeys();

                Assert.True(cache.Count <= capacity);

                var expectedCachedItem = dic.Where(m => m.Value != 0).ToArray();
                if (removeFlag)
                {
                    Assert.True(cache.Count == capacity);
                    Assert.True(expectedCachedItem.Length == capacity + 1);
                    var expectedRemoveItem = expectedCachedItem.Where(m => m.Key != num)
                        .MinBy(m => m.Value).OrderBy(m => m.Key).First();

                    var removedKeys = keys.Except(newKeys).ToArray();
                    Assert.Single(removedKeys);
                    Assert.Equal(expectedRemoveItem.Key, removedKeys[0]);

                    var addedKeys = newKeys.Except(keys).ToArray();
                    Assert.Single(addedKeys);
                    Assert.Equal(num, addedKeys[0]);

                    dic[expectedRemoveItem.Key] = 0;
                }
                else
                {
                    var actualKeys = newKeys.OrderBy(m => m).ToArray();
                    var expectKeys = expectedCachedItem.Select(m => m.Key).OrderBy(m => m).ToArray();
                    Assert.True(expectKeys.SequenceEqual(actualKeys));
                }
            }
        }
    }
}
