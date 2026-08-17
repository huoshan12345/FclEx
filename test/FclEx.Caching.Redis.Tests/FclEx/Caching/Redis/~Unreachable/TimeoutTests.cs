namespace FclEx.Caching.Redis._Unreachable;

public class TimeoutTests(RedisUnreachableTestsFixture fixture) : RedisUnreachableTests(fixture)
{
    public static FieldInfo FieldOfRedisOptions { get; } = typeof(DefaultRedisCachingProvider).GetRequiredField("_options");

    [Fact]
    public void SetTimeout_Test()
    {
        var options = Services.GetOptions<RedisOptions>().DbOptions;
        var actualOptions = FieldOfRedisOptions.GetRequiredValue<EasyCaching.Redis.RedisOptions>(RedisCachingProvider);
        Assert.Single(actualOptions.DBConfig.Endpoints);
        Assert.Equal(options.ConnectionTimeout, actualOptions.DBConfig.ConnectionTimeout);
    }

    [RetryFact]
    public async Task WaitTimeout_Test()
    {
        var options = Services.GetOptions<RedisOptions>().DbOptions;
        var timeout = options.ConnectionTimeout;
        var (successful, _, _, elapsed) = await Operation.ExecuteAsync(t => EasyCachingProvider.GetAsync<string>("test", t), TimeSpan.FromMilliseconds(timeout)).Unwrap();
        Assert.False(successful);
        Assert.True(elapsed.TotalMilliseconds < timeout + 500, elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture));
    }
}