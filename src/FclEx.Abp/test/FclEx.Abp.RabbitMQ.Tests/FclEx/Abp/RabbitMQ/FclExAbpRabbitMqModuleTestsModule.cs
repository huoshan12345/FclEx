using Volo.Abp.Modularity;

namespace FclEx.Abp.RabbitMQ;

[DependsOn(typeof(FclExAbpRabbitMqModule))]
public class FclExAbpRabbitMqModuleTestsModule : AbpModule
{
}