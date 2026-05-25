namespace FclEx.Redis;

public interface IRedisSet : IRedisCollection
{
    Task<long> SCardAsync();
}

public interface IRedisSet<T> : IRedisSet, IRedisCollection<T>
{
    Task<long> SAddAsync(IList<T> cacheValues);
    Task<bool> SIsMemberAsync(T cacheValue);
    Task<List<T>> SMembersAsync();
    Task<T> SPopAsync();
    Task<List<T>> SRandMemberAsync(int count = 1);
    Task<long> SRemAsync(IList<T>? cacheValues = null);
}