using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using FclEx.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.DependencyInjection;

namespace FclEx.Abp.DependencyInjection
{
    public class OpenGenericConventionalRegistrar : DefaultConventionalRegistrar
    {
        public override void AddAssembly(IServiceCollection services, Assembly assembly)
        {
            var types = GetAllTypes(assembly)
                .Where(type => type != null
                               && type.IsClass
                               && !type.IsAbstract
                               && type.IsGenericType
                               && !type.IsDefined(typeof(CompilerGeneratedAttribute), true)
                               ).ToArray();
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

            var serviceTypes = ExposedServiceExplorer.GetExposedServices(type);

            TriggerServiceExposing(services, type, serviceTypes);

            foreach (var serviceType in serviceTypes)
            {
                var t = IsOpenGenericType(serviceType)
                    ? serviceType.GetGenericTypeDefinition()
                    : serviceType;

                var serviceDescriptor = ServiceDescriptor.Describe(t, type, lifeTime.Value);

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
                    services.AddIfNotExist(serviceDescriptor);
                }
            }
        }

        private static bool IsOpenGenericType(Type type)
        {
            return type.IsGenericType
                   && !type.IsGenericTypeDefinition
                   && type.GenericTypeArguments.Any(x => x.IsGenericParameter);
        }

        public static IReadOnlyList<Type> GetAllTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types!;
            }
        }
    }
}
