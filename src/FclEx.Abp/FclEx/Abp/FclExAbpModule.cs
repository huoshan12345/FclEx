using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Activity.Providers;

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
        // to fix https://github.com/abpframework/abp/issues/23755
        // and disable abp telemetry
        context.Services
            .Remove(m => m.ServiceType == typeof(ITelemetryActivityEventBuilder))
            .Remove(m => m.ServiceType == typeof(TelemetryActivityEventBuilder))
            .AddSingleton<ITelemetryActivityEventBuilder, NullTelemetryActivityEventBuilder>()
            .Remove(m => m.ServiceType == typeof(ITelemetryService))
            .Remove(m => m.ServiceType == typeof(TelemetryService))
            .AddSingleton<ITelemetryService, NullTelemetryService>();

        context.Services.AddFclExCaching();
    }
}