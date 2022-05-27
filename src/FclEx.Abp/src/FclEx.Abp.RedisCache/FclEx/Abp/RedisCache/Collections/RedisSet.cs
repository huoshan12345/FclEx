using System.Collections.Generic;
using System.Threading.Tasks;
using EasyCaching.Core;
using FclEx.Abp.Caching.Configuration;

namespace FclEx.Abp.RedisCache.Collections
{
    internal class RedisSet<T> : RedisCol<T>, IRedisSet<T>
    {
        public RedisSet(string name, IRedisCachingProvider provider, AbpCacheOptions options)
            : base(name, provider, options)
        {
        }

        public long SCard() => _provider.SCard(Key);
        public Task<long> SCardAsync() => _provider.SCardAsync(Key);
        public long SAdd(IList<T> cacheValues) => _provider.SAdd(Key, cacheValues);
        public bool SIsMember(T cacheValue) => _provider.SIsMember(Key, cacheValue);
        public List<T> SMembers() => _provider.SMembers<T>(Key);
        public T SPop() => _provider.SPop<T>(Key);
        public List<T> SRandMember(int count = 1) => _provider.SRandMember<T>(Key, count);
        public long SRem(IList<T>? cacheValues = null) => _provider.SRem<T>(Key, cacheValues);
        public Task<long> SAddAsync(IList<T> cacheValues) => _provider.SAddAsync<T>(Key, cacheValues);
        public Task<bool> SIsMemberAsync(T cacheValue) => _provider.SIsMemberAsync<T>(Key, cacheValue);
        public Task<List<T>> SMembersAsync() => _provider.SMembersAsync<T>(Key);
        public Task<T> SPopAsync() => _provider.SPopAsync<T>(Key);
        public Task<List<T>> SRandMemberAsync(int count = 1) => _provider.SRandMemberAsync<T>(Key, count);
        public Task<long> SRemAsync(IList<T>? cacheValues = null) => _provider.SRemAsync<T>(Key, cacheValues);
        public override RedisColType ColType { get; } = RedisColType.Set;
    }
}
