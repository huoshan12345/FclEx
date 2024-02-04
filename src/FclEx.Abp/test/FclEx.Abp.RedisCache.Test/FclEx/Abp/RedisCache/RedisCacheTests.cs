using EasyCaching.Core.Serialization;
using EasyCaching.Serialization.Json;
using EasyCaching.Serialization.MessagePack;
using FclEx.Abp.Caching;

namespace FclEx.Abp.RedisCache;

using FclEx.Extensions;

public class RedisCacheTests
{
    private readonly ITestOutputHelper _output;
    public RedisCacheTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public static readonly IEnumerable<object[]> TwoDimensionalBools = new[] { true, false }.SelectMany((x, y) => new object[] { x, y });

    [Theory]
    [MemberData(nameof(TwoDimensionalBools))]
    public void TestCache(bool useMessagePack, bool serializeStringAsRaw)
    {
        var tests = AbpRedisTests.Build(_output, useMessagePack, serializeStringAsRaw);
        var serviceProvider = tests.ServiceProvider;

        const string str = "test";
        var provider = serviceProvider.GetRequiredService<IEasyCachingProvider>();
        Assert.IsType<DefaultCSRedisCachingProvider>(provider);
        var cacheManager = serviceProvider.GetRequiredService<ICacheManager>();
        var cache = cacheManager.GetCache<string>(str);
        var obj = cache.Get(str, k => str);
        Assert.True(obj.HasValue);
        var objNew = cache.Get(str);
        Assert.True(objNew.HasValue);
        Assert.Equal(obj.Value, objNew.Value);

        cache.Remove(str);
        var objRemoved = cache.Get(str);
        Assert.False(objRemoved.HasValue);
    }

    private static void Test<T>(AbpRedisTests tests, string key, T value)
    {
        var provider = tests.ServiceProvider.GetRequiredService<IEasyCachingProvider>();
        key = key.ToLower();
        provider.Remove(key);

        var obj = provider.Get(key, () => value, TimeSpan.FromMinutes(1));
        Assert.True(obj.HasValue);
        Assert.Equal(value, obj.Value);

        var objNew = provider.Get<T>(key);
        Assert.True(objNew.HasValue);
        Assert.Equal(value, objNew.Value);

        if (typeof(T) == typeof(string) && tests.AbpRedisOptions.SerializeStringAsRaw)
        {
            var client = tests.ServiceProvider.GetRequiredService<EasyCachingCSRedisClient>();
            var valueOfRaw = client.Get<T>(key); 
            Assert.Equal(value, valueOfRaw);
        }
    }

    [Theory]
    [MemberData(nameof(TwoDimensionalBools))]
    public void StringAsRaw_Test(bool useMessagePack, bool serializeStringAsRaw)
    {
        var tests = AbpRedisTests.Build(_output, useMessagePack, serializeStringAsRaw);
        var serviceProvider = tests.ServiceProvider;
        var abpCacheOptions = tests.AbpCacheOptions;

        var provider = serviceProvider.GetRequiredService<IEasyCachingProvider>();
        Assert.IsType<DefaultCSRedisCachingProvider>(provider);

        var serializer = serviceProvider.GetRequiredService<IEasyCachingSerializer>();
        if (serializeStringAsRaw)
            Assert.IsType<StringAsRawEasyCachingSerializer>(serializer);
        else if (useMessagePack)
            Assert.IsType<DefaultMessagePackSerializer>(serializer);
        else
            Assert.IsType<DefaultJsonSerializer>(serializer);

        var datas = Enumerable.Range(1, 10)
            .Select((m, i) => new CacheTester() { Id = m, Age = m, Name = m.ToString("D8") })
            .ToArray();

        foreach (var data in datas)
        {
            Test(tests, nameof(CacheTester) + abpCacheOptions.Separator + data.Name, data); // class
            Test(tests, nameof(CacheTester.Name) + abpCacheOptions.Separator + data.Name, data.Name); // string
            Test(tests, nameof(CacheTester.Age) + abpCacheOptions.Separator + data.Age, data.Age); // int
        }
    }

    [Theory]
    [MemberData(nameof(TwoDimensionalBools))]
    public void GetAll_Test(bool useMessagePack, bool serializeStringAsRaw)
    {
        var tests = AbpRedisTests.Build(_output, useMessagePack, serializeStringAsRaw);
        var serviceProvider = tests.ServiceProvider;
        var abpCacheOptions = tests.AbpCacheOptions;

        var cacheManager = serviceProvider.GetRequiredService<ICacheManager>();
        var cache = cacheManager.GetCache<string>("number");
        var keys = Enumerable.Range(1, 10).Select(m => m.ToString()).ToArray();
        cache.RemoveAll(keys);
        foreach (var key in keys)
        {
            Assert.False(cache.Exists(key));
            cache.Set(key, key + key, TimeSpan.FromHours(1));
        }
        var all = cache.GetAll(keys);

        foreach (var key in keys)
        {
            Assert.True(all.TryGetValue(key, out var value));
            Assert.True(value.HasValue);
            Assert.Equal(key + key, value.Value);
        }
    }

    [Theory]
    [MemberData(nameof(TwoDimensionalBools))]
    public async Task GetAllAsync_Test(bool useMessagePack, bool serializeStringAsRaw)
    {
        var tests = AbpRedisTests.Build(_output, useMessagePack, serializeStringAsRaw);
        var serviceProvider = tests.ServiceProvider;
        var abpCacheOptions = tests.AbpCacheOptions;

        var cacheManager = serviceProvider.GetRequiredService<ICacheManager>();
        var cache = cacheManager.GetCache<string>("number");
        var keys = Enumerable.Range(1, 10).Select(m => m.ToString()).ToArray();
        await cache.RemoveAllAsync(keys);
        foreach (var key in keys)
        {
            Assert.False(await cache.ExistsAsync(key));
            await cache.SetAsync(key, key + key, TimeSpan.FromHours(1));
        }
        var all = await cache.GetAllAsync(keys);

        foreach (var key in keys)
        {
            Assert.True(all.TryGetValue(key, out var value));
            Assert.True(value.HasValue);
            Assert.Equal(key + key, value.Value);
        }
    }
}