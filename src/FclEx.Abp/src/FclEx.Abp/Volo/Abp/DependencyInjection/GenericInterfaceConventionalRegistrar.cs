using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Volo.Abp.DependencyInjection;

/// <summary>
/// Register non-generic classes like <strong>Service</strong> with generic interfaces like <strong>IService&lt;int&gt;</strong><br/>
/// NOTE: this feature has been implemented in Volo.Abp already.
/// </summary>
public class GenericInterfaceConventionalRegistrar : DefaultConventionalRegistrar
{
    public override void AddType(IServiceCollection services, Type type)
    {
        if (IsConventionalRegistrationDisabled(type))
            return;

        var dependencyAttribute = GetDependencyAttributeOrNull(type);
        var lifeTime = GetLifeTimeOrNull(type, dependencyAttribute);

        if (lifeTime == null)
            return;

        var serviceTypes = GetGenericInterfaces(type);
        TriggerServiceExposing(services, type, serviceTypes);

        foreach (var serviceType in serviceTypes)
        {
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
                services.TryAdd(serviceDescriptor);
            }
        }
    }

    private static List<Type> GetGenericInterfaces(Type type)
    {
        var serviceTypes = new List<Type>();

        foreach (var interfaceType in type.GetInterfaces().Where(m => m.IsGenericType))
        {
            var interfaceName = interfaceType.SimpleName().TrimStart("I");
            if (type.Name.EndsWith(interfaceName))
            {
                serviceTypes.Add(interfaceType);
            }
        }

        return serviceTypes;
    }
}