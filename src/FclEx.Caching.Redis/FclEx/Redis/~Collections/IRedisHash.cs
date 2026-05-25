namespace FclEx.Redis;

public interface IRedisHash : IRedisCollection
{
    Task<bool> HExistsAsync(string field);
    Task<long> HDelAsync(IList<string>? fields = null);
    Task<long> HIncrByAsync(string field, long val = 1);
    Task<List<string>> HKeysAsync();
    Task<long> HLenAsync();
}

public interface IRedisHash<T> : IRedisHash, IRedisCollection<T>
{
    Task<T> HGetAsync(string field);
    Task<bool> HSetAsync(string field, T cacheValue);
    Task<bool> HmSetAsync(Dictionary<string, T> vals, TimeSpan? expiration = null);
    Task<Dictionary<string, T>> HGetAllAsync();
    Task<List<T>> HValsAsync();
    Task<Dictionary<string, T>> HmGetAsync(IList<string> fields);
}