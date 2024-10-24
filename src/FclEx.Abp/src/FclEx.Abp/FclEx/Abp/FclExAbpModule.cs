using FclEx.Abp.Caching;
using FclEx.Abp.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectMapping;

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
            .AddEasyCaching(o => o.UseInMemory().WithJson())
            .AddSingleton<ICacheManager, CacheManager>()
            .AddSingleton<IStringSerializer>(StringAsRawSerializer.Instance);
    }
}