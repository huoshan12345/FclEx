using Volo.Abp.Modularity;

namespace FclEx.Abp.RedisCache;

[DependsOn(typeof(FclExAbpRedisModule))]
public class AbpRedisTestModule : AbpModule
{
    public const string RedisConStrName = "Redis";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var config = context.Services.GetConfiguration();
        context.Services.Configure<AbpRedisOptions>(config.GetRequiredSection(RedisConStrName));
        context.Services.Configure<AbpRedisOptions>(m =>
        {
            m.ConStrs.ForEach(x => x.DefaultDatabase = Environment.Version.Major);
            m.ConfigureAllCollections(x => x.UseGlobalPrefix = true);
        });
    }
}