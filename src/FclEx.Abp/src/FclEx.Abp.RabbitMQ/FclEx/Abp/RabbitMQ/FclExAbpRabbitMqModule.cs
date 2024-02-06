using Volo.Abp.Modularity;

namespace FclEx.Abp.RabbitMQ;

[DependsOn(typeof(FclExAbpModule))]
public class FclExAbpRabbitMqModule : AbpModule
{
}