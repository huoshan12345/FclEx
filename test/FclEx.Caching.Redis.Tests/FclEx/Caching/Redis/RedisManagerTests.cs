using StackExchange.Redis;

namespace FclEx.Caching.Redis;

public class RedisManagerTests(RedisTestsFixture fixture) : RedisTests(fixture)
{
    [RetryFact]
    public async Task GetList_Test()
    {
        var key = nameof(GetList_Test).ToLower();
        var col = RedisManager.GetList<string>(key);

        var colKey = col.Key;
        if (await RedisCachingProvider.KeyExistsAsync(colKey))
            await RedisCachingProvider.KeyDelAsync(colKey);

        Assert.Equal(RedisCollectionType.List, col.CollectionType);
        Assert.Equal(colKey, col.Key);

        await col.LPushAsync(key);
        Assert.Equal(1, await col.LLenAsync());
        Assert.True(await RedisCachingProvider.KeyExistsAsync(colKey));
        Assert.Equal(1, await RedisCachingProvider.LLenAsync(col.Key));
    }

    [RetryFact]
    public async Task GetHash_Test()
    {
        var key = nameof(GetHash_Test).ToLower();
        var col = RedisManager.GetHash<string>(key);

        var colKey = col.Key;
        if (await RedisCachingProvider.KeyExistsAsync(colKey))
            await RedisCachingProvider.KeyDelAsync(colKey);

        Assert.Equal(RedisCollectionType.Hash, col.CollectionType);
        Assert.Equal(colKey, col.Key);

        await col.HSetAsync(key, key);
        Assert.Equal(1, await col.HLenAsync());

        Assert.True(await RedisCachingProvider.KeyExistsAsync(colKey));
        Assert.Equal(1, await RedisCachingProvider.HLenAsync(col.Key));
    }

    [RetryFact]
    public async Task GetSet_Test()
    {
        var key = nameof(GetSet_Test).ToLower();
        var col = RedisManager.GetSet<string>(key);

        var colKey = col.Key;
        if (await RedisCachingProvider.KeyExistsAsync(colKey))
            await RedisCachingProvider.KeyDelAsync(colKey);

        Assert.Equal(RedisCollectionType.Set, col.CollectionType);
        Assert.Equal(colKey, col.Key);

        await col.SAddAsync(key);

        Assert.Equal(1, await col.SCardAsync());
        Assert.True(await RedisCachingProvider.KeyExistsAsync(colKey));
        Assert.Equal(1, await RedisCachingProvider.SCardAsync(col.Key));
    }

    [RetryFact]
    public async Task GetSortedSet_Test()
    {
        var key = nameof(GetSortedSet_Test).ToLower();
        var database = EasyCachingProvider.Database.CastTo<IDatabase>();

        var col = RedisManager.GetSortedSet<string>(key);

        var colKey = col.Key;
        if (await RedisCachingProvider.KeyExistsAsync(colKey))
            await RedisCachingProvider.KeyDelAsync(colKey);

        Assert.Equal(RedisCollectionType.SortedSet, col.CollectionType);
        Assert.Equal(colKey, col.Key);

        await col.ZAddAsync(key, 1);

        Assert.Equal(1, await database.SortedSetLengthAsync(colKey));
        Assert.Equal(1, await col.ZCountAsync(0, 10));
        Assert.True(await RedisCachingProvider.KeyExistsAsync(colKey));
        Assert.Equal(1, await RedisCachingProvider.ZCountAsync(colKey, 0, 10));
    }
}