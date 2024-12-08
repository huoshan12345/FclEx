using EasyCaching.Serialization.SystemTextJson.Configurations;

namespace FclEx.Abp;

[DependsOn(typeof(AbpAutoMapperModule))]
public class FclExAbpModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddConventionalRegistrar(new OpenGenericConventionalRegistrar());
        // context.Services.AddConventionalRegistrar(new GenericInterfaceConventionalRegistrar());
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMaps(GetType().Assembly)
            .AddEasyCaching(o => o.UseInMemory().WithSystemTextJson())
            .AddSingleton<ICacheManager, CacheManager>()
            .AddSingleton<IStringSerializer>(StringAsRawSerializer.Instance);
    }
}