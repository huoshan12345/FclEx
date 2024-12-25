namespace FclEx.Abp.RedisCache;

public class TimeoutTests(ITestOutputHelper output) : AbpRedisUnreachableTests(output)
{
    public static FieldInfo FieldOfRedisOptions { get; } = typeof(DefaultRedisCachingProvider).GetRequiredField("_options");

    [Fact]
    public void SetTimeout_Test()
    {
        var options = ServiceProvider.GetOptions<AbpRedisOptions>().RedisOptions;
        var provider = ServiceProvider.GetRequiredService<IEasyCachingProvider>();
        Assert.IsType<PatchedRedisCachingProvider>(provider);
        var redisProvider = (PatchedRedisCachingProvider)provider;
        var actualOptions = FieldOfRedisOptions.GetRequiredValue<RedisOptions>(redisProvider);
        Assert.Single(actualOptions.DBConfig.Endpoints);
        Assert.Equal(options.ConnectionTimeout, actualOptions.DBConfig.ConnectionTimeout);
    }

    [RetryFact]
    public async Task WaitTimeout_Test()
    {
        var options = ServiceProvider.GetOptions<AbpRedisOptions>();
        var provider = ServiceProvider.GetRequiredService<IEasyCachingProvider>();
        var timeout = options.RedisOptions.ConnectionTimeout;
        var (successful, _, _, elapsed) = await Operate.ExecuteAsync(() => provider.GetAsync<string>("test"), TimeSpan.FromMilliseconds(timeout)).Unwrap();
        Assert.False(successful);
        Assert.True(elapsed.TotalMilliseconds < timeout + 500, elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture));
    }
}