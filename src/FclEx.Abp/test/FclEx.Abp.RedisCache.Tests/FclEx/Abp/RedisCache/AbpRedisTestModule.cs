using Volo.Abp.Modularity;

namespace FclEx.Abp.RedisCache;

[DependsOn(typeof(FclExAbpRedisModule))]
public class AbpRedisTestModule : AbpModule
{
    public const string RedisSectionKey = "Redis";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var config = context.Services.GetConfiguration();
        var options = config.GetRequiredValue<RedisDBOptions>(RedisSectionKey);
        context.Services.Configure<AbpRedisOptions>(m =>
        {
            m.RedisOptions = options;
            m.RedisOptions.Database = Environment.Version.Major;
            m.ConfigureAllCollections(x => x.UseGlobalPrefix = true);
        });
    }
}