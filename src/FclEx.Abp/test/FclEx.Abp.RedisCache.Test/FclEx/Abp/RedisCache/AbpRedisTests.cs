using FclEx.Abp.Caching.Configuration;
using FclEx.Abp.RedisCache.Configuration;
using FclEx.Abp.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace FclEx.Abp.RedisCache
{
    public class AbpRedisTests : AbpAopTests<AbpRedisTestModule>
    {
        private readonly Lazy<AbpRedisOptions> _abpRedisOptions;
        public AbpRedisOptions AbpRedisOptions => _abpRedisOptions.Value;

        private readonly Lazy<AbpCacheOptions> _abpCacheOptions;
        public AbpCacheOptions AbpCacheOptions => _abpCacheOptions.Value;

        protected override IConfigurationRoot BuildConfig()
        {
            return GlobalConstants.Config;
        }

        public AbpRedisTests(ITestOutputHelper output, Action<IServiceCollection> action = null)
            : base(output, action)
        {
            _abpRedisOptions = new Lazy<AbpRedisOptions>(() => ServiceProvider.GetOptions<AbpRedisOptions>(), true);
            _abpCacheOptions = new Lazy<AbpCacheOptions>(() => ServiceProvider.GetOptions<AbpCacheOptions>(), true);
        }

        public static AbpRedisTests Build(ITestOutputHelper output, bool useMessagePack, bool serializeStringAsRaw)
        {
            return new AbpRedisTests(output, s => s.Configure<AbpRedisOptions>(o =>
            {
                o.UseMessagePack = useMessagePack;
                o.SerializeStringAsRaw = serializeStringAsRaw;
            }));
        }
    }
}
