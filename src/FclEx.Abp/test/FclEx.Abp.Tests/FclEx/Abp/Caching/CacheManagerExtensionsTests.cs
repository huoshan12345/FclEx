namespace FclEx.Abp.Caching;

public class CacheManagerExtensionsTests : AbpTests<AbpTestModule>
{
    public CacheManagerExtensionsTests(ITestOutputHelper output, Action<AbpTestsOptions>? action = null)
        : base(output, action)
    {
    }

    [Fact]
    public async Task GetObjectAsync_Raw_Test()
    {
        var cacheManager = ServiceProvider.GetRequiredService<ICacheManager>();
        const string expectedValue = "value";
        const string cacheName = nameof(GetObjectAsync_Raw_Test);
        const string cacheKey = "key";
        var (successful, result, _, _) = await cacheManager.GetObjectAsync(() => Task.FromResult(expectedValue),
            cacheKey, cacheName, TimeSpan.FromSeconds(10));
        Assert.True(successful);
        Assert.Equal(expectedValue, result);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.True(cache.TryGet(cacheKey, out var valueInCache));
        Assert.Equal(expectedValue, valueInCache);

        await cache.RemoveAsync(cacheKey);
    }

    [Fact]
    public async Task GetObjectAsync_Raw_Fail()
    {
        var cacheManager = ServiceProvider.GetRequiredService<ICacheManager>();
        const string cacheName = nameof(GetObjectAsync_Raw_Fail);
        const string cacheKey = "key";
        var (successful, _, ex, _) = await cacheManager.GetObjectAsync<string>(() => throw new InvalidOperationException(),
            cacheKey, cacheName, TimeSpan.FromSeconds(10));

        Assert.False(successful);
        Assert.IsType<InvalidOperationException>(ex);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.False(cache.TryGet(cacheKey, out _));
    }

    [Fact]
    public async Task GetObjectAsync_OperateResult_Test()
    {
        var cacheManager = ServiceProvider.GetRequiredService<ICacheManager>();
        const string expectedValue = "value";
        const string cacheName = nameof(GetObjectAsync_OperateResult_Test);
        const string cacheKey = "key";
        var (successful, result, _, _) = await cacheManager.GetObjectAsync(() => Task.FromResult(Operate.CreateSuccess(expectedValue)),
            cacheKey, cacheName, TimeSpan.FromSeconds(10));
        Assert.True(successful);
        Assert.Equal(expectedValue, result);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.True(cache.TryGet(cacheKey, out var valueInCache));
        Assert.Equal(expectedValue, valueInCache);

        await cache.RemoveAsync(cacheKey);
    }

    [Fact]
    public async Task GetObjectAsync_OperateResult_Fail()
    {
        var cacheManager = ServiceProvider.GetRequiredService<ICacheManager>();
        const string cacheName = nameof(GetObjectAsync_OperateResult_Fail);
        const string cacheKey = "key";
        var (successful, _, ex, _) = await cacheManager.GetObjectAsync<string>(() => Operate.CreateError<string>(new InvalidOperationException()).ToTask(),
            cacheKey, cacheName, TimeSpan.FromSeconds(10));

        Assert.False(successful);
        Assert.IsType<InvalidOperationException>(ex);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.False(cache.TryGet(cacheKey, out _));
    }

    [Fact]
    public async Task SetObjectAsync_Raw_Test()
    {
        var cacheManager = ServiceProvider.GetRequiredService<ICacheManager>();
        const string expectedValue = "value";
        const string cacheName = nameof(SetObjectAsync_Raw_Test);
        const string cacheKey = "key";
        var (successful, result, _, _) = await cacheManager.SetObjectAsync(() => Task.FromResult(expectedValue),
            cacheKey, cacheName, TimeSpan.FromSeconds(10));
        Assert.True(successful);
        Assert.Equal(expectedValue, result);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.True(cache.TryGet(cacheKey, out var valueInCache));
        Assert.Equal(expectedValue, valueInCache);

        await cache.RemoveAsync(cacheKey);
    }

    [Fact]
    public async Task SetObjectAsync_Raw_Fail()
    {
        var cacheManager = ServiceProvider.GetRequiredService<ICacheManager>();
        const string cacheName = nameof(SetObjectAsync_Raw_Fail);
        const string cacheKey = "key";
        var (successful, _, ex, _) = await cacheManager.SetObjectAsync<string>(() => throw new InvalidOperationException(),
            cacheKey, cacheName, TimeSpan.FromSeconds(10));

        Assert.False(successful);
        Assert.IsType<InvalidOperationException>(ex);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.False(cache.TryGet(cacheKey, out _));
    }

    [Fact]
    public async Task SetObjectAsync_OperateResult_Test()
    {
        var cacheManager = ServiceProvider.GetRequiredService<ICacheManager>();
        const string expectedValue = "value";
        const string cacheName = nameof(SetObjectAsync_Raw_Test);
        const string cacheKey = "key";
        var (successful, result, _, _) = await cacheManager.SetObjectAsync(() => Task.FromResult(Operate.CreateSuccess(expectedValue)),
            cacheKey, cacheName, TimeSpan.FromSeconds(10));
        Assert.True(successful);
        Assert.Equal(expectedValue, result);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.True(cache.TryGet(cacheKey, out var valueInCache));
        Assert.Equal(expectedValue, valueInCache);

        await cache.RemoveAsync(cacheKey);
    }

    [Fact]
    public async Task SetObjectAsync_OperateResult_Fail()
    {
        var cacheManager = ServiceProvider.GetRequiredService<ICacheManager>();
        const string cacheName = nameof(SetObjectAsync_OperateResult_Fail);
        const string cacheKey = "key";
        var (successful, _, ex, _) = await cacheManager.SetObjectAsync<string>(() => Operate.CreateError<string>(new InvalidOperationException()).ToTask(),
            cacheKey, cacheName, TimeSpan.FromSeconds(10));

        Assert.False(successful);
        Assert.IsType<InvalidOperationException>(ex);

        var cache = cacheManager.GetCache<string>(cacheName);
        Assert.False(cache.TryGet(cacheKey, out _));
    }
}