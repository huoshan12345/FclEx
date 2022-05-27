using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRedis;
using EasyCaching.Core;
using FclEx.Abp.RedisCache.Collections;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Abp.RedisCache
{
    public class RedisHashTests : AbpRedisTests
    {
        public RedisHashTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void HSet_HGet_String_Test()
        {
            var key = nameof(HSet_HGet_String_Test).ToLower();
            var manager = ServiceProvider.GetRequiredService<IRedisColManager>();
            var col = manager.GetHash<string>(key);

            col.HSet("1", "11");
            var actual = col.HGet("1");
            Assert.Equal("11", actual);
        }


        [Fact]
        public void Provider_HmSet_HmGet_String_Test()
        {
            var key = nameof(Provider_HmSet_HmGet_String_Test).ToLower();
            var manager = ServiceProvider.GetRequiredService<IRedisColManager>();
            var col = manager.GetHash<string>(key);
            var keyExt = col.Key;
            var provider = ServiceProvider.GetRequiredService<IRedisCachingProvider>();
            var dic = Enumerable.Range(1, 10)
                .Select(m => m.ToString())
                .ToDictionary(m => m, m => m + m);

            provider.HMSet(keyExt, dic, TimeSpan.FromMinutes(1));
            var actual = provider.HMGet(keyExt, dic.Keys.ToList());
            Assert.Equal(dic, actual);
        }

        [Fact]
        public void HmSet_HmGet_String_Test()
        {
            var key = nameof(HmSet_HmGet_String_Test).ToLower();
            var manager = ServiceProvider.GetRequiredService<IRedisColManager>();
            var col = manager.GetHash<string>(key);

            var dic = Enumerable.Range(1, 10)
                .Select(m => m.ToString())
                .ToDictionary(m => m, m => m + m);

            col.HmSet(dic, TimeSpan.FromMinutes(1));
            var actual = col.HmGet(dic.Keys.ToList());
            Assert.Equal(dic, actual);
        }


        [Fact]
        public async Task HmSetAsync_HmGetAsync_String_Test()
        {
            var key = nameof(HmSetAsync_HmGetAsync_String_Test).ToLower();
            var manager = ServiceProvider.GetRequiredService<IRedisColManager>();
            var col = manager.GetHash<string>(key);

            var dic = Enumerable.Range(1, 10)
                .Select(m => m.ToString())
                .ToDictionary(m => m, m => m + m);

            await col.HmSetAsync(dic, TimeSpan.FromMinutes(1));
            var actual = await col.HmGetAsync(dic.Keys.ToList());
            Assert.Equal(dic, actual);
        }
    }
}
