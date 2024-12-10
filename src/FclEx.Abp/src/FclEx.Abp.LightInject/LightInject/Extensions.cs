using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace LightInject;

public static class Extensions
{
    private static MethodInfo CreateFunc { get; } = typeof(Extensions)
        .GetMethod(nameof(CreateTypedFactoryDelegate),
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic)!;

    public static IServiceContainer RegisterMsDiService(this IServiceContainer container, IServiceCollection serviceCollection)
    {
        foreach (var service in serviceCollection)
        {
            RegisterMsDiService(container, service);
        }
        return container;
    }

    public static string GetNewName(this IServiceContainer services, Type serviceType)
    {
        var count = services.AvailableServices.Count(m => m.ServiceType == serviceType);

        var name = serviceType.Namespace + "_"
                                         + serviceType.ShortName() + "_"
                                         + count.ToString("D8", CultureInfo.InvariantCulture.NumberFormat);
        return name;
    }

    public static string GetNewName<TImpl>(this IServiceContainer services)
    {
        return services.GetNewName(typeof(TImpl));
    }

    public static IServiceContainer RegisterMsDiService(this IServiceContainer container, ServiceDescriptor service)
    {
        var registration = ToServiceRegistration(service);
        registration.ServiceName = container.GetNewName(registration.ServiceType);
        container.Register(registration);
        return container;
    }

    public static IServiceContainer AddMsDiScope(this IServiceContainer container)
    {
        if (container.AvailableServices.All(m => m.ServiceType != typeof(IServiceProvider)))
        {
            container.RegisterInstance<IServiceProvider>(new LightInjectServiceProvider(container));
        }
        container.RegisterInstance<IServiceScopeFactory>(new LightInjectServiceScopeFactory(container));
        return container;
    }

    public static ILifetime? ToLightInjectLifetime(this ServiceLifetime lifeStyle)
    {
        return lifeStyle switch
        {
            ServiceLifetime.Singleton => (ILifetime)new PerContainerLifetime(),
            ServiceLifetime.Scoped => new PerScopeLifetime(),
            ServiceLifetime.Transient => null,
            _ => throw new ArgumentOutOfRangeException(nameof(lifeStyle), lifeStyle, null)
        };
    }

    public static ServiceRegistration ToServiceRegistration(this ServiceDescriptor service)
    {
        if (service.ImplementationFactory != null)
        {
            return CreateServiceRegistrationForFactoryDelegate(service);
        }
        if (service.ImplementationInstance != null)
        {
            return CreateServiceRegistrationForInstance(service);
        }
        return CreateServiceRegistrationServiceType(service);
    }

    private static ServiceRegistration CreateServiceRegistrationServiceType(ServiceDescriptor serviceDescriptor)
    {
        var registration = CreateBasicServiceRegistration(serviceDescriptor);
        registration.ImplementingType = serviceDescriptor.ImplementationType;
        return registration;
    }

    private static ServiceRegistration CreateServiceRegistrationForInstance(ServiceDescriptor serviceDescriptor)
    {
        var registration = CreateBasicServiceRegistration(serviceDescriptor);
        registration.Value = serviceDescriptor.ImplementationInstance;
        return registration;
    }

    private static ServiceRegistration CreateServiceRegistrationForFactoryDelegate(ServiceDescriptor serviceDescriptor)
    {
        var registration = CreateBasicServiceRegistration(serviceDescriptor);
        registration.FactoryExpression = CreateFactoryDelegate(serviceDescriptor);
        return registration;
    }

    private static ServiceRegistration CreateBasicServiceRegistration(ServiceDescriptor serviceDescriptor)
    {
        var registration = new ServiceRegistration
        {
            Lifetime = serviceDescriptor.Lifetime.ToLightInjectLifetime(),
            ServiceType = serviceDescriptor.ServiceType,
        };
        return registration;
    }

    private static Delegate CreateFactoryDelegate(ServiceDescriptor serviceDescriptor)
    {
        var closedGenericMethod = CreateFunc.MakeGenericMethod(serviceDescriptor.ServiceType);
        return (Delegate)closedGenericMethod.Invoke(null, [serviceDescriptor])!;
    }

    private static Func<IServiceFactory, T> CreateTypedFactoryDelegate<T>(ServiceDescriptor serviceDescriptor)
    {
        return serviceFactory => (T)serviceDescriptor.ImplementationFactory!(serviceFactory.GetInstance<IServiceProvider>());
    }
}