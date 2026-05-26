namespace FclEx.Caching.Redis;

public class RedisHashTests(RedisTestsFixture fixture) : RedisTests(fixture)
{
    [RetryFact]
    public async Task Provider_HmSetAsync_HmGetAsync_String_Test()
    {
        var key = nameof(Provider_HmSetAsync_HmGetAsync_String_Test).ToLower();
        var manager = Services.GetRequiredService<IRedisManager>();
        var col = manager.GetHash<string>(key);
        var colKey = col.Key;
        var dic = Enumerable.Range(1, 10)
            .Select(m => m.ToString())
            .ToDictionary(m => m, m => m + m);

        await RedisCachingProvider.HMSetAsync(colKey, dic, TimeSpan.FromMinutes(1));
        var actual = await RedisCachingProvider.HMGetAsync(colKey, dic.Keys.ToList());
        Assert.Equal(dic, actual);
    }

    [RetryFact]
    public async Task HSetAsync_HGetAsync_String_Test()
    {
        var key = nameof(HSetAsync_HGetAsync_String_Test).ToLower();
        var col = RedisManager.GetHash<string>(key);

        await col.HSetAsync("1", "11");
        var actual = await col.HGetAsync("1");
        Assert.Equal("11", actual);
    }

    [RetryFact]
    public async Task HmSetAsync_HmGetAsync_String_Test()
    {
        var key = nameof(HmSetAsync_HmGetAsync_String_Test).ToLower();
        var manager = Services.GetRequiredService<IRedisManager>();
        var col = manager.GetHash<string>(key);

        var dic = Enumerable.Range(1, 10)
            .Select(m => m.ToString())
            .ToDictionary(m => m, m => m + m);

        await col.HmSetAsync(dic, TimeSpan.FromMinutes(1));
        var actual = await col.HmGetAsync(dic.Keys.ToList());
        Assert.Equal(dic, actual);
    }
}