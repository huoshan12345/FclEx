namespace FclEx.Abp.RedisCache;

public class AbpRedisUnreachableTests(ITestOutputHelper output) : AbpTests<AbpRedisTestModule>(output)
{
    protected override IConfigurationRoot BuildConfig()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Unreachable.json", false, false)
            .Build();
    }
}