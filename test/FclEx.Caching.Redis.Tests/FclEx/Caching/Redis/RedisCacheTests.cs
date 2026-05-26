namespace FclEx.Caching.Redis;

public class RedisCacheTests(RedisTestsFixture fixture) : RedisTests(fixture)
{
    public record TestModel(int Id, string? Name, int Age, int? CoinCount = null);

    private async Task TestAsync<T>(string key, T value)
    {
        key = key.ToLower();
        await EasyCachingProvider.RemoveAsync(key);

        var obj = await EasyCachingProvider.GetAsync(key, () => Task.FromResult(value), TimeSpan.FromMinutes(1));
        Assert.True(obj.HasValue);
        Assert.Equal(value, obj.Value);

        var objNew = await EasyCachingProvider.GetAsync<T>(key);
        Assert.True(objNew.HasValue);
        Assert.Equal(value, objNew.Value);
    }

    [Fact]
    public void Type_Test()
    {
        Assert.IsType<PatchedRedisCachingProvider>(EasyCachingProvider);
        Assert.IsType<PatchedJsonSerializer>(EasyCachingSerializer);
    }

    [RetryFact]
    public async Task Basic_Test()
    {
        var name = nameof(Basic_Test) + Environment.Version.Major;

        var cache = CacheManager.GetCache<string>(name);
        var obj = await cache.GetAsync(name, k => Task.FromResult(name));
        Assert.True(obj.HasValue);
        var objNew = await cache.GetAsync(name);
        Assert.True(objNew.HasValue);
        Assert.Equal(obj.Value, objNew.Value);

        await cache.RemoveAsync(name);
        var objRemoved = await cache.GetAsync(name);
        Assert.False(objRemoved.HasValue);
    }

    [Fact]
    public async Task Serializer_Test()
    {
        var array = Enumerable.Range(1, 3)
            .Select((m, i) => new TestModel(m, m.ToString("D8"), m))
            .ToArray();

        var sep = CacheManagerOptions.Separator;
        foreach (var value in array)
        {
            await TestAsync(nameof(TestModel) + sep + value.Name, value); // class
            await TestAsync(nameof(TestModel.Name) + sep + value.Name, value.Name); // string
            await TestAsync(nameof(TestModel.Age) + sep + value.Age, value.Age); // int
        }
    }

    [RetryFact]
    public async Task GetAll_Test()
    {
        var cache = CacheManager.GetCache<string>(nameof(GetAll_Test) + Environment.Version.Major);
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

    [RetryFact]
    public async Task GetAllAsync_Test()
    {
        var cache = CacheManager.GetCache<string>(nameof(GetAllAsync_Test) + Environment.Version.Major);
        var keys = Enumerable.Range(1, 3).Select(m => m.ToString()).ToArray();
        await cache.RemoveAllAsync(keys);
        foreach (var key in keys)
        {
            var exist = await cache.ExistsAsync(key);
            Assert.False(exist, key);
            await cache.SetAsync(key, key + key, TimeSpan.FromHours(1));
        }
        var all = await cache.GetAllAsync(keys);

        foreach (var key in keys)
        {
            Assert.True(all.TryGetValue(key, out var value), () => $"keys: {all.Keys.JoinWith(" ")}, cache key: {cache.Prefix}");
            Assert.True(value!.HasValue);
            Assert.Equal(key + key, value.Value);
        }
    }
}