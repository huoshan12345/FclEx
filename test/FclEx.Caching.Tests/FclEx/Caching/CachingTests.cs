namespace FclEx.Caching;

[CollectionDefinition(nameof(CachingTestsCollection))]
public class CachingTestsCollection : ICollectionFixture<CachingTestsFixture>;

[Collection(nameof(CachingTestsCollection))]
public class CachingTests
{
    public static IServiceProvider Services => CachingTestsFixture.Services;
    public static ICacheManager CacheManager => CachingTestsFixture.CacheManager;
}