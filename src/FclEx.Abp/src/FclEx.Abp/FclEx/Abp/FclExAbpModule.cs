using EasyCaching.Serialization.SystemTextJson;

namespace FclEx.Abp;

public class FclExAbpModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddConventionalRegistrar(new OpenGenericConventionalRegistrar());
        // context.Services.AddConventionalRegistrar(new GenericInterfaceConventionalRegistrar());
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services
            .AddEasyCaching(o => o.UseInMemory().WithPatchedSystemTextJson())
            .AddSingleton<ICacheManager, CacheManager>()
            .AddSingleton<IStringSerializer>(StringAsRawSerializer.Instance);
    }
}