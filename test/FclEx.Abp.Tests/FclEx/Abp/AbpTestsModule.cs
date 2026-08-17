using Volo.Abp.Modularity;

namespace FclEx.Abp;

[DependsOn(typeof(FclExAbpModule))]
public class AbpTestsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        
    }
}
