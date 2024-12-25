using System;
using FclEx.Extensions;

namespace FclEx.Abp.RedisCache;

internal class RedisHash<T> : RedisCollection<T>, IRedisHash<T>
{
    private readonly IStringSerializer _stringSerializer;

    public RedisHash(string name, 
        IRedisCachingProvider provider, 
        AbpCacheOptions options, 
        IStringSerializer stringSerializer) 
        : base(name, provider, options)
    {
        _stringSerializer = stringSerializer;
    }

    public override RedisCollectionType CollectionType { get; } = RedisCollectionType.Hash;
    public bool HSet(string field, T cacheValue)
    {
        var str = _stringSerializer.Serialize(cacheValue);
        return _provider.HSet(Key, field, str);
    }

    public bool HExists(string field) => _provider.HExists(Key, field);
    public long HDel(IList<string>? fields = null) => _provider.HDel(Key, fields);
    public long HIncrBy(string field, long val = 1) => _provider.HIncrBy(Key, field, val);
    public List<string> HKeys() => _provider.HKeys(Key);
    public long HLen() => _provider.HLen(Key);
    public Task<bool> HExistsAsync(string field) => _provider.HExistsAsync(Key, field);
    public Task<long> HDelAsync(IList<string>? fields = null) => _provider.HDelAsync(Key, fields);
    public Task<long> HIncrByAsync(string field, long val = 1) => _provider.HIncrByAsync(Key, field, val);
    public Task<List<string>> HKeysAsync() => _provider.HKeysAsync(Key);
    public Task<long> HLenAsync() => _provider.HLenAsync(Key);

    public T HGet(string field)
    {
        var str = _provider.HGet(Key, field);
        return _stringSerializer.Deserialize<T>(str)!;
    }

    public bool HmSet(Dictionary<string, T> vals, TimeSpan? expiration = null)
    {
        var dic = vals.ToDictionary(m => m.Key, m => _stringSerializer.Serialize(m.Value));
        return _provider.HMSet(Key, dic, expiration ?? DefaultExpiration);
    }

    public Dictionary<string, T> HGetAll()
    {
        var vals = _provider.HGetAll(Key);
        var dic = vals.ToDictionary(m => m.Key, m => _stringSerializer.Deserialize<T>(m.Value));
        return dic!;
    }

    public List<T> HVals()
    {
        var vals = _provider.HVals(Key);
        var list = vals.Select(m => _stringSerializer.Deserialize<T>(m)).ToList();
        return list!;
    }

    public Dictionary<string, T> HmGet(IList<string> fields)
    {
        var vals = _provider.HMGet(Key, fields);
        var dic = vals.ToDictionary(m => m.Key, m => _stringSerializer.Deserialize<T>(m.Value));
        return dic!;
    }

    public Task<bool> HSetAsync(string field, T cacheValue)
    {
        var str = _stringSerializer.Serialize(cacheValue);
        return _provider.HSetAsync(Key, field, str);
    }

    public async Task<T> HGetAsync(string field)
    {
        var str = await _provider.HGetAsync(Key, field).IgnoreSyncContext();
        return _stringSerializer.Deserialize<T>(str)!;
    }

    public Task<bool> HmSetAsync(Dictionary<string, T> vals, TimeSpan? expiration = null)
    {
        var dic = vals.ToDictionary(m => m.Key, m => _stringSerializer.Serialize(m.Value));
        return _provider.HMSetAsync(Key, dic, expiration ?? DefaultExpiration);
    }

    public async Task<Dictionary<string, T>> HGetAllAsync()
    {
        var vals = await _provider.HGetAllAsync(Key).IgnoreSyncContext();
        var dic = vals.ToDictionary(m => m.Key, m => _stringSerializer.Deserialize<T>(m.Value));
        return dic!;
    }

    public async Task<List<T>> HValsAsync()
    {
        var vals = await _provider.HValsAsync(Key).IgnoreSyncContext();
        var list = vals.Select(m => _stringSerializer.Deserialize<T>(m)).ToList();
        return list!;
    }

    public async Task<Dictionary<string, T>> HmGetAsync(IList<string> fields)
    {
        var vals = await _provider.HMGetAsync(Key, fields).IgnoreSyncContext();
        var dic = vals.ToDictionary(m => m.Key, m => _stringSerializer.Deserialize<T>(m.Value));
        return dic!;
    }
}