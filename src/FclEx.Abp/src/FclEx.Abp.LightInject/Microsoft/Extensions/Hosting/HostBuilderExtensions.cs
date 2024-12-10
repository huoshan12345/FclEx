using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Extensions.LightInject;
using LightInject;

namespace Microsoft.Extensions.Hosting;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseLightInject(this IHostBuilder hostBuilder, Action<LightInjectOptions>? optionsBuilder = null)
    {
        var (container, fac) = LightInjectHelper.CreateContainerAndFactory(optionsBuilder);
        return hostBuilder.ConfigureServices((_, services) => services.AddObjectAccessor<IServiceContainer>(container))
            .UseServiceProviderFactory(fac);
    }

    public static IHostBuilder UseLightInject(this IHostBuilder hostBuilder, bool useAop)
    {
        return hostBuilder.UseLightInject(o => o.UseAop = useAop);
    }
}