namespace FclEx.Caching;

public class CachingTestsFixture : CoreTestsFixture
{
    public static readonly IServiceProvider Services = new ServiceCollection()
        .AddFclExCaching()
        .BuildServiceProvider();

    public static ICacheManager CacheManager => Services.GetRequiredService<ICacheManager>();
}
