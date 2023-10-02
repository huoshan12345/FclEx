using System;
using System.Collections.Generic;
using System.Text;
using LightInject;
using Microsoft.Extensions.DependencyInjection;

namespace Volo.Abp;

public static class AbpExtensions
{
    public static T? GetObject<T>(this ApplicationInitializationContext context)
    {
        return context.ServiceProvider.GetObject<T>();
    }

    public static void UseLightInject(this AbpApplicationCreationOptions options, Action<LightInjectOptions>? optionsBuilder = null)
    {
        var (container, fac) = LightInjectHelper.CreateContainerAndFactory(optionsBuilder);
        options.Services.AddObjectAccessor<IServiceContainer>(container);
        options.Services.AddSingleton(fac);
    }

    public static void UseLightInject(this AbpApplicationCreationOptions options, bool useAop)
    {
        options.UseLightInject(o => o.UseAop = useAop);
    }
}