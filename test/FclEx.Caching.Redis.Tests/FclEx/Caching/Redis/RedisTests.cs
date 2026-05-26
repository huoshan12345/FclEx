using EasyCaching.Core.Serialization;

namespace FclEx.Caching.Redis;

[CollectionDefinition(nameof(AbpRedisTestsCollection))]
public class AbpRedisTestsCollection : ICollectionFixture<RedisTestsFixture>;

[EnableParallelization]
[Collection(nameof(AbpRedisTestsCollection))]
public class RedisTests(RedisTestsFixture fixture)
{
    public static ITestOutputHelper? Output => TestContext.Current.TestOutputHelper;
    public static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public RedisTestsFixture Fixture => fixture;
    public IServiceProvider Services => Fixture.Services;
    public IRedisCachingProvider RedisCachingProvider => Services.GetRequiredService<IRedisCachingProvider>();
    public IRedisManager RedisManager => Services.GetRequiredService<IRedisManager>();
    public IEasyCachingProvider EasyCachingProvider => Services.GetRequiredService<IEasyCachingProvider>();
    public ICacheManager CacheManager => Services.GetRequiredService<ICacheManager>();
    public IEasyCachingSerializer EasyCachingSerializer => Services.GetRequiredService<IEasyCachingSerializer>();
    public RedisOptions RedisOptions => Services.GetOptions<RedisOptions>();
    public CacheManagerOptions CacheManagerOptions => Services.GetOptions<CacheManagerOptions>();
}