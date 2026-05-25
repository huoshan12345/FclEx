namespace FclEx.Redis;

public interface IRedisList : IRedisCollection
{
    Task<long> LLenAsync();
    Task<bool> LTrimAsync(long start, long stop);
}

public interface IRedisList<T> : IRedisList, IRedisCollection<T>
{
    Task<T> LIndexAsync(long index);
    Task<T> LPopAsync();
    Task<long> LPushAsync(IList<T> cacheValues);
    Task<List<T>> LRangeAsync(long start, long stop);
    Task<long> LRemAsync(long count, T cacheValue);
    Task<bool> LSetAsync(long index, T cacheValue);
    Task<long> LPushXAsync(T cacheValue);
    Task<long> LInsertBeforeAsync(T pivot, T cacheValue);
    Task<long> LInsertAfterAsync(T pivot, T cacheValue);
    Task<long> RPushAsync(IList<T> cacheValues);
    Task<T> RPopAsync();
}