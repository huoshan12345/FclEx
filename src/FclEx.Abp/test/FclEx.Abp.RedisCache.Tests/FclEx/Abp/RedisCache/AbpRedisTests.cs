using FclEx.Abp.Caching.Configuration;
using Volo.Abp;

namespace FclEx.Abp.RedisCache;

public class AbpRedisTests : AbpAopTests<AbpRedisTestModule>
{
    private readonly Lazy<AbpRedisOptions> _abpRedisOptions;
    public AbpRedisOptions AbpRedisOptions => _abpRedisOptions.Value;

    private readonly Lazy<AbpCacheOptions> _abpCacheOptions;
    public AbpCacheOptions ReadOnlyCacheOptions => _abpCacheOptions.Value;

    private readonly Action<IServiceCollection>? _action;

    protected AbpRedisTests(ITestOutputHelper output, Action<IServiceCollection>? action = null) : base(output)
    {
        _action = action;
        _abpRedisOptions = new Lazy<AbpRedisOptions>(() => ServiceProvider.GetOptions<AbpRedisOptions>(), true);
        _abpCacheOptions = new Lazy<AbpCacheOptions>(() => ServiceProvider.GetOptions<AbpCacheOptions>(), true);
    }

    protected override IConfigurationRoot BuildConfig()
    {
        return GlobalConstants.Config;
    }

    protected override void Configure(AbpApplicationCreationOptions options, IConfigurationRoot configuration)
    {
        base.Configure(options, configuration);
        _action?.Invoke(options.Services);
    }
}