using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Check = FclEx.Check;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        internal static MethodInfo MethodOfAddHostedService { get; } = typeof(ServiceCollectionHostedServiceExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(m => m.Name == nameof(ServiceCollectionHostedServiceExtensions.AddHostedService))
            .Where(m =>
            {
                var paras = m.GetParameters();
                return paras.Length == 1 && paras[0].ParameterType == typeof(IServiceCollection);
            }).First();

        public static IServiceCollection AddHostedService(this IServiceCollection services, Type type)
        {
            Check.NotNull(services);
            Check.NotNull(type);
            return (IServiceCollection)MethodOfAddHostedService.MakeGenericMethod(type).Invoke(null, new object[] { services })!;
        }

        public static IServiceCollection AddHostedService(this IServiceCollection services, Assembly assembly, Func<Type, bool>? filter = null)
        {
            Check.NotNull(services);
            var types = assembly.GetTypes()
                .Where(m => m.IsClass && !m.IsAbstract)
                .Where(m => typeof(IHostedService).IsAssignableFrom(m));
            if (filter != null)
                types = types.Where(filter);
            foreach (var type in types)
            {
                services.AddHostedService(type);
            }
            return services;
        }

        public static IServiceCollection AddHostedService(this IServiceCollection services, Assembly assembly, params Type[] excludeTypes)
        {
            return services.AddHostedService(assembly, m => !excludeTypes.Contains(m));
        }

        public static IServiceCollection AddMaps(this IServiceCollection services, Assembly assembly, bool validate = false)
        {
            return services.Configure<AbpAutoMapperOptions>(options => options.AddMaps(assembly, validate));
        }
        
        public static T GetOptions<T>(this IServiceCollection services) where T : class, new()
            => services.BuildServiceProvider().GetRequiredService<IOptions<T>>().Value;

        /// <summary>
        /// Combine of <see cref="AddAbp{TStartupModule}"/> and <see cref="ServiceProviderExtensions.UseAbp"/>
        /// </summary>
        /// <typeparam name="TStartupModule"></typeparam>
        /// <param name="services"></param>
        /// <param name="optionsAction"></param>
        /// <returns></returns>
        public static IServiceProvider BuildAbp<TStartupModule>(this IServiceCollection services, Action<AbpApplicationCreationOptions>? optionsAction = null)
            where TStartupModule : IAbpModule
        {
            var app = services.AddApplication<TStartupModule>(optionsAction);
            app.Initialize(services.BuildServiceProviderFromFactory()); // if lightinject is used, the provider will be created from factory
            return app.ServiceProvider;
        }

        public static IServiceCollection AddAbp<TStartupModule>(this IServiceCollection services, Action<AbpApplicationCreationOptions>? optionsAction = null)
            where TStartupModule : IAbpModule
        {
            services.AddApplication<TStartupModule>(optionsAction);
            return services;
        }

        public static IServiceProvider UseAbp(this IServiceCollection services)
        {
            var provider = services.BuildServiceProviderFromFactory();
            provider.GetRequiredService<IAbpApplicationWithExternalServiceProvider>().Initialize(provider);
            return provider;
        }
    }
}
