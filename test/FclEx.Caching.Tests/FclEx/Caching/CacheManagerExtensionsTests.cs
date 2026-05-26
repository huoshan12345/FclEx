namespace FclEx.Caching;

public class CacheManagerExtensionsTests : CachingTests
{
    [RetryFact]
    public async Task GetOrCreateAsync_Test()
    {
        var cacheManager = Services.GetRequiredService<ICacheManager>();
        const string expectedValue = "value";
        const string cacheName = nameof(GetOrCreateAsync_Test);
        const string cacheKey = "key";
        var (successful, result, _, _) = await cacheManager.GetOrCreateAsync(cacheName, cacheKey,
            () => Task.FromResult(expectedValue), TimeSpan.FromSeconds(10));
        Assert.True(successful);
        Assert.Equal(expectedValue, result);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.True(cache.TryGet(cacheKey, out var valueInCache));
        Assert.Equal(expectedValue, valueInCache);

        await cache.RemoveAsync(cacheKey);
    }

    [RetryFact]
    public async Task GetOrCreateAsync_Fail()
    {
        var cacheManager = Services.GetRequiredService<ICacheManager>();
        const string cacheName = nameof(GetOrCreateAsync_Fail);
        const string cacheKey = "key";
        var (successful, _, ex, _) = await cacheManager.GetOrCreateAsync<string>(cacheName, cacheKey,
            () => throw new InvalidOperationException(), TimeSpan.FromSeconds(10));

        Assert.False(successful);
        Assert.IsType<InvalidOperationException>(ex);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.False(cache.TryGet(cacheKey, out _));
    }

    [RetryFact]
    public async Task GetOrCreateAsync_OperationResult_Test()
    {
        var cacheManager = Services.GetRequiredService<ICacheManager>();
        const string expectedValue = "value";
        const string cacheName = nameof(GetOrCreateAsync_OperationResult_Test);
        const string cacheKey = "key";
        var (successful, result, _, _) = await cacheManager.GetOrCreateAsync(cacheName, cacheKey,
            () => Task.FromResult(Operation.Success(expectedValue)), TimeSpan.FromSeconds(10));
        Assert.True(successful);
        Assert.Equal(expectedValue, result);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.True(cache.TryGet(cacheKey, out var valueInCache));
        Assert.Equal(expectedValue, valueInCache);

        await cache.RemoveAsync(cacheKey);
    }

    [RetryFact]
    public async Task GetOrCreateAsync_OperationResult_Fail()
    {
        var cacheManager = Services.GetRequiredService<ICacheManager>();
        const string cacheName = nameof(GetOrCreateAsync_OperationResult_Fail);
        const string cacheKey = "key";
        var (successful, _, ex, _) = await cacheManager.GetOrCreateAsync<string>(cacheName, cacheKey,
            () => Task.FromResult(Operation.Error<string>(new InvalidOperationException())), TimeSpan.FromSeconds(10));

        Assert.False(successful);
        Assert.IsType<InvalidOperationException>(ex);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.False(cache.TryGet(cacheKey, out _));
    }

    [RetryFact]
    public async Task SetAsync_Test()
    {
        var cacheManager = Services.GetRequiredService<ICacheManager>();
        const string expectedValue = "value";
        const string cacheName = nameof(SetAsync_Test);
        const string cacheKey = "key";
        var (successful, result, _, _) = await cacheManager.SetAsync(cacheName, cacheKey,
            () => Task.FromResult(expectedValue), TimeSpan.FromSeconds(10));
        Assert.True(successful);
        Assert.Equal(expectedValue, result);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.True(cache.TryGet(cacheKey, out var valueInCache));
        Assert.Equal(expectedValue, valueInCache);

        await cache.RemoveAsync(cacheKey);
    }

    [RetryFact]
    public async Task SetAsync_Fail()
    {
        var cacheManager = Services.GetRequiredService<ICacheManager>();
        const string cacheName = nameof(SetAsync_Fail);
        const string cacheKey = "key";
        var (successful, _, ex, _) = await cacheManager.SetAsync<string>(cacheName, cacheKey,
            () => throw new InvalidOperationException(), TimeSpan.FromSeconds(10));

        Assert.False(successful);
        Assert.IsType<InvalidOperationException>(ex);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.False(cache.TryGet(cacheKey, out _));
    }

    [RetryFact]
    public async Task SetAsync_OperationResult_Test()
    {
        var cacheManager = Services.GetRequiredService<ICacheManager>();
        const string expectedValue = "value";
        const string cacheName = nameof(SetAsync_Test);
        const string cacheKey = "key";
        var (successful, result, _, _) = await cacheManager.SetAsync(cacheName, cacheKey,
            () => Task.FromResult(Operation.Success(expectedValue)), TimeSpan.FromSeconds(10));
        Assert.True(successful);
        Assert.Equal(expectedValue, result);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.True(cache.TryGet(cacheKey, out var valueInCache));
        Assert.Equal(expectedValue, valueInCache);

        await cache.RemoveAsync(cacheKey);
    }

    [RetryFact]
    public async Task SetAsync_OperationResult_Fail()
    {
        var cacheManager = Services.GetRequiredService<ICacheManager>();
        const string cacheName = nameof(SetAsync_OperationResult_Fail);
        const string cacheKey = "key";
        var (successful, _, ex, _) = await cacheManager.SetAsync<string>(cacheName, cacheKey,
            () => Task.FromResult(Operation.Error<string>(new InvalidOperationException())), TimeSpan.FromSeconds(10));

        Assert.False(successful);
        Assert.IsType<InvalidOperationException>(ex);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.False(cache.TryGet(cacheKey, out _));
    }
}