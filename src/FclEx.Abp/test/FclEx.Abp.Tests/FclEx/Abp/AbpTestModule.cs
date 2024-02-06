using Volo.Abp.Modularity;

namespace FclEx.Abp;

[DependsOn(typeof(FclExAbpModule))]
public class AbpTestModule : AbpModule
{
}