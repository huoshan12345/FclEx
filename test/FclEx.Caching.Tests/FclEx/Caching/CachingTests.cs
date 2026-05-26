using Meziantou.Xunit.v3;

namespace FclEx.Caching;

[CollectionDefinition(nameof(CachingTestsCollection))]
public class CachingTestsCollection : ICollectionFixture<CachingTestsFixture>;

[EnableParallelization]
[Collection(nameof(CachingTestsCollection))]
public class CachingTests
{
    public static IServiceProvider Services => CachingTestsFixture.Services;
    public static ICacheManager CacheManager => CachingTestsFixture.CacheManager;
}