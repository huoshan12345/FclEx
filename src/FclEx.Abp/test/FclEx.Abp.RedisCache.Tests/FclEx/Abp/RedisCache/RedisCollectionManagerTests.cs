using FclEx.Abp.RedisCache.Collections;
using StackExchange.Redis;

namespace FclEx.Abp.RedisCache;

public class RedisCollectionManagerTests(ITestOutputHelper output) : AbpRedisTests(output)
{
    [Fact]
    public void GetList_Test()
    {
        var key = nameof(GetList_Test).ToLower();
        var manager = ServiceProvider.GetRequiredService<IRedisCollectionManager>();
        var provider = ServiceProvider.GetRequiredService<IRedisCachingProvider>();
        var col = manager.GetList<string>(key);

        var keyExt = col.Key;
        if (provider.KeyExists(keyExt))
            provider.KeyDel(keyExt);

        Assert.Equal(RedisCollectionType.List, col.CollectionType);
        Assert.Equal(keyExt, col.Key);

        col.LPush(key);
        Assert.Equal(1, col.LLen());
        Assert.True(provider.KeyExists(keyExt));
        Assert.Equal(1, provider.LLen(col.Key));
    }

    [Fact]
    public void GetHash_Test()
    {
        var key = nameof(GetHash_Test).ToLower();
        var manager = ServiceProvider.GetRequiredService<IRedisCollectionManager>();
        var provider = ServiceProvider.GetRequiredService<IRedisCachingProvider>();
        var col = manager.GetHash<string>(key);

        var keyExt = col.Key;
        if (provider.KeyExists(keyExt))
            provider.KeyDel(keyExt);

        Assert.Equal(RedisCollectionType.Hash, col.CollectionType);
        Assert.Equal(keyExt, col.Key);

        col.HSet(key, key);
        Assert.Equal(1, col.HLen());

        Assert.True(provider.KeyExists(keyExt));
        Assert.Equal(1, provider.HLen(col.Key));
    }

    [Fact]
    public void GetSet_Test()
    {
        var key = nameof(GetSet_Test).ToLower();
        var manager = ServiceProvider.GetRequiredService<IRedisCollectionManager>();
        var provider = ServiceProvider.GetRequiredService<IRedisCachingProvider>();
        var col = manager.GetSet<string>(key);

        var keyExt = col.Key;
        if (provider.KeyExists(keyExt))
            provider.KeyDel(keyExt);

        Assert.Equal(RedisCollectionType.Set, col.CollectionType);
        Assert.Equal(keyExt, col.Key);

        col.SAdd(key);
        Assert.Equal(1, col.SCard());

        Assert.True(provider.KeyExists(keyExt));
        Assert.Equal(1, provider.SCard(col.Key));
    }

    [Fact]
    public void GetSortedSet_Test()
    {
        var key = nameof(GetSortedSet_Test).ToLower();
        var manager = ServiceProvider.GetRequiredService<IRedisCollectionManager>();
        var provider = ServiceProvider.GetRequiredService<IRedisCachingProvider>();
        var database = ServiceProvider.GetRequiredService<IEasyCachingProvider>().Database.CastTo<IDatabase>();

        var col = manager.GetSortedSet<string>(key);

        var keyExt = col.Key;
        if (provider.KeyExists(keyExt))
            provider.KeyDel(keyExt);

        Assert.Equal(RedisCollectionType.SortedSet, col.CollectionType);
        Assert.Equal(keyExt, col.Key);

        col.ZAdd(key, 1);
        Assert.Equal(1, database.SortedSetLength(keyExt));



        Assert.Equal(1, col.ZCount(0, 10));

        Assert.True(provider.KeyExists(keyExt));
        Assert.Equal(1, provider.ZCount(keyExt, 0, 10));
    }
}