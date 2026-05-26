using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Volo.Abp.DependencyInjection;

/// <summary>
/// Register open generic classes like <strong>Service&lt;T&gt;</strong>
/// </summary>
public class OpenGenericConventionalRegistrar : DefaultConventionalRegistrar
{
    public override void AddAssembly(IServiceCollection services, Assembly assembly)
    {
        var types = GetAllTypes(assembly)
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: true }
                           && !type.IsDefined(typeof(CompilerGeneratedAttribute), true))
            .ToArray();
        AddTypes(services, types);
    }

    public override void AddType(IServiceCollection services, Type type)
    {
        if (IsConventionalRegistrationDisabled(type))
            return;

        var dependencyAttribute = GetDependencyAttributeOrNull(type);
        var lifeTime = GetLifeTimeOrNull(type, dependencyAttribute);

        if (lifeTime == null)
            return;

        var serviceTypes = GetExposedServices(type);

        TriggerServiceExposing(services, type, serviceTypes);

        foreach (var serviceType in serviceTypes)
        {
            // NOTE: an open generic type cannot be redirected to implementation type
            var serviceDescriptor = ServiceDescriptor.Describe(serviceType, type, lifeTime.Value);

            if (dependencyAttribute?.ReplaceServices == true)
            {
                services.Replace(serviceDescriptor);
            }
            else if (dependencyAttribute?.TryRegister == true)
            {
                services.TryAdd(serviceDescriptor);
            }
            else
            {
                services.Add(serviceDescriptor);
            }
        }
    }

    private static List<Type> GetExposedServices(Type type)
    {
        var serviceTypes = new List<Type> { type };

        foreach (var interfaceType in type.GetTypeInfo().GetInterfaces())
        {
            var interfaceName = interfaceType.Name.TrimStart("I");

            // NOTE: we use type.Name instead of type.SimpleName() here to ensure the interface type has the same generic typeDefinition
            if (type.Name.EndsWith(interfaceName))
            {
                serviceTypes.Add(interfaceType.GetGenericTypeDefinition());
            }
        }

        return serviceTypes;
    }

    public static IReadOnlyList<Type> GetAllTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.NotNull().ToArray();
        }
    }
}