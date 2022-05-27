using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace FclEx.Abp.OrmLite
{
    [DependsOn(typeof(FclExAbpOrmLiteModule))]
    public class AbpOrmLiteTestModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.Configure<AbpOrmLiteOptions>(o =>
                o.ConStrs.Add(new OrmLiteConStr(GlobalConstants.MainConStrKey, "", new EmptyOrmLiteDialectProvider())));
        }
    }
}
