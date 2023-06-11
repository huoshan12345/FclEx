using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace LightInject;

public class LightInjectServiceProviderFactory : IServiceProviderFactory<IServiceContainer>
{
    private readonly IServiceContainer _serviceContainer;

    public LightInjectServiceProviderFactory(IServiceContainer serviceContainer)
    {
        _serviceContainer = serviceContainer;
    }

    public IServiceContainer CreateBuilder(IServiceCollection services)
    {
        _serviceContainer.RegisterMsDiService(services);
        return _serviceContainer;
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