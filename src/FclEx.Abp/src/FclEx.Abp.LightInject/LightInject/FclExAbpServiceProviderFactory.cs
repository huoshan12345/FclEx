using System;

namespace LightInject;

public class LightInjectServiceProviderFactory(IServiceContainer serviceContainer) : IServiceProviderFactory<IServiceContainer>
{
    public IServiceContainer CreateBuilder(IServiceCollection services)
    {
        serviceContainer.RegisterMsDiService(services);
        return serviceContainer;
    }

    public IServiceProvider CreateServiceProvider(IServiceContainer containerBuilder)
    {
        return containerBuilder.GetInstance<IServiceProvider>();
    }

    public static IServiceProviderFactory<IServiceContainer> Create(IServiceContainer serviceContainer)
    {
        return new LightInjectServiceProviderFactory(serviceContainer);
    }
}