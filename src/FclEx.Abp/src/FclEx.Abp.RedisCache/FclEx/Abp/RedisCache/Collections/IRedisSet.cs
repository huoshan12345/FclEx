using System.Collections.Generic;
using System.Threading.Tasks;

namespace FclEx.Abp.RedisCache.Collections
{
    public interface IRedisSet : IRedisCol
    {
        long SCard();
        Task<long> SCardAsync();
    }

    public interface IRedisSet<T> : IRedisSet, IRedisCol<T>
    {
        long SAdd(IList<T> cacheValues);
        bool SIsMember(T cacheValue);
        List<T> SMembers();
        T SPop();
        List<T> SRandMember(int count = 1);
        long SRem(IList<T>? cacheValues = null);
        Task<long> SAddAsync(IList<T> cacheValues);
        Task<bool> SIsMemberAsync(T cacheValue);
        Task<List<T>> SMembersAsync();
        Task<T> SPopAsync();
        Task<List<T>> SRandMemberAsync(int count = 1);
        Task<long> SRemAsync(IList<T>? cacheValues = null);
    }
}
