namespace FclEx.Abp.RedisCache;

public class RedisCacheTests(ITestOutputHelper output) : AbpRedisTests(output)
{
    public record Model(int Id, string? Name, int Age, int? CoinCount = null);
    
    private void Test<T>(string key, T value)
    {
        var provider = ServiceProvider.GetRequiredService<IEasyCachingProvider>();
        key = key.ToLower();
        provider.Remove(key);

        var obj = provider.Get(key, () => value, TimeSpan.FromMinutes(1));
        Assert.True(obj.HasValue);
        Assert.Equal(value, obj.Value);

        var objNew = provider.Get<T>(key);
        Assert.True(objNew.HasValue);
        Assert.Equal(value, objNew.Value);
    }

    [Fact]
    public void Basic_Test()
    {
        const string str = "test";
        var provider = ServiceProvider.GetRequiredService<IEasyCachingProvider>();
        Assert.IsType<PatchedRedisCachingProvider>(provider);
        var cacheManager = ServiceProvider.GetRequiredService<ICacheManager>();
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

    [Fact]
    public void Serializer_Test()
    {
        var provider = ServiceProvider.GetRequiredService<IEasyCachingProvider>();
        Assert.IsType<PatchedRedisCachingProvider>(provider);

        var serializer = ServiceProvider.GetRequiredService<IEasyCachingSerializer>();
        Assert.IsType<PatchedJsonSerializer>(serializer);

        var array = Enumerable.Range(1, 3)
            .Select((m, i) => new Model(m, m.ToString("D8"), m))
            .ToArray();

        var sep = ReadOnlyCacheOptions.Separator;
        foreach (var value in array)
        {
            Test(nameof(Model) + sep + value.Name, value); // class
            Test(nameof(Model.Name) + sep + value.Name, value.Name); // string
            Test(nameof(Model.Age) + sep + value.Age, value.Age); // int
        }
    }

    [Fact]
    public void GetAll_Test()
    {
        var cacheManager = ServiceProvider.GetRequiredService<ICacheManager>();
        var cache = cacheManager.GetCache<string>("number");
        var keys = Enumerable.Range(1, 3).Select(m => m.ToString()).ToArray();
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

    [Fact]
    public async Task GetAllAsync_Test()
    {
        var cacheManager = ServiceProvider.GetRequiredService<ICacheManager>();
        var cache = cacheManager.GetCache<string>("number");
        var keys = Enumerable.Range(1, 3).Select(m => m.ToString()).ToArray();
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