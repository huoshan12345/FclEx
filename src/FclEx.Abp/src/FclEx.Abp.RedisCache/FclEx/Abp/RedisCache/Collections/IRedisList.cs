namespace FclEx.Abp.RedisCache.Collections;

public interface IRedisList : IRedisCollection
{
    long LLen();
    Task<long> LLenAsync();
    Task<bool> LTrimAsync(long start, long stop);
}

public interface IRedisList<T> : IRedisList, IRedisCollection<T>
{
    T LIndex(long index);
    T LPop();
    long LPush(IList<T> cacheValues);
    List<T> LRange(long start, long stop);
    long LRem(long count, T cacheValue);
    bool LSet(long index, T cacheValue);
    bool LTrim(long start, long stop);
    long LPushX(T cacheValue);
    long LInsertBefore(T pivot, T cacheValue);
    long LInsertAfter(T pivot, T cacheValue);
    long RPushX(T cacheValue);
    long RPush(IList<T> cacheValues);
    T RPop();
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