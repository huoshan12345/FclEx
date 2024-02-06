namespace FclEx.Abp.RedisCache;

public class TimeoutTests : AbpRedisUnreachableTests
{
    public static FieldInfo FieldOfRedisOptions { get; } = typeof(DefaultCSRedisCachingProvider).GetRequiredField("_options");

    public static readonly Regex RegOfConTimeout = new(@"connectTimeout=(\d)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public TimeoutTests(ITestOutputHelper output, Action<AbpTestsOptions>? action = null)
        : base(output, action)
    {
    }

    [Fact]
    public void SetTimeout_Test()
    {
        var (_, _, conStrs, _) = ServiceProvider.GetOptions<AbpRedisOptions>();
        var con = conStrs.Single();
        var provider = ServiceProvider.GetRequiredService<IEasyCachingProvider>();
        Assert.IsType<DefaultCSRedisCachingProvider>(provider);
        var csRedisProvider = (DefaultCSRedisCachingProvider)provider;
        var actualOptions = FieldOfRedisOptions.GetRequiredValue<RedisOptions>(csRedisProvider);
        Assert.Single(actualOptions!.DBConfig.ConnectionStrings);
        var str = actualOptions.DBConfig.ConnectionStrings.First();

        if (RegOfConTimeout.TryMatch(str, 1, out var value))
        {
            Assert.Equal(con.ConnectTimeout / 1000, int.Parse(value));
        }
        else
        {
            Assert.True(false);
        }
    }

    [RetryFact]
    public async Task WaitTimeout_Test()
    {
        var (_, _, conStrs, _) = ServiceProvider.GetOptions<AbpRedisOptions>();
        var con = conStrs.Single();
        var provider = ServiceProvider.GetRequiredService<IEasyCachingProvider>();
        var timeout = con.ConnectTimeout;
        var (successful, _, _, elapsed) = await Operate.ExecuteAsync(() => provider.GetAsync<string>("test"), TimeSpan.FromMilliseconds(timeout)).Unwrap();
        Assert.False(successful);
        Assert.True(elapsed.TotalMilliseconds < timeout + 1000, elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture));
    }
}