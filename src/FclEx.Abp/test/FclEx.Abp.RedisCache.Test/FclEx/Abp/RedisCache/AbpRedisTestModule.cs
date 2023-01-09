using FclEx.Abp.RedisCache.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace FclEx.Abp.RedisCache
{
    [DependsOn(typeof(FclExAbpRedisModule))]
    public class AbpRedisTestModule : AbpModule
    {
        public const string RedisConStrName = "ConnectionStrings:Redis";

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var config = context.Services.GetConfiguration();
            context.Services.Configure<AbpRedisOptions>(config.GetRequiredSection(RedisConStrName));
            context.Services.Configure<AbpRedisOptions>(m => m.ConfigureAll(x => x.UseGlobalPrefix = true));
        }
    }
}
