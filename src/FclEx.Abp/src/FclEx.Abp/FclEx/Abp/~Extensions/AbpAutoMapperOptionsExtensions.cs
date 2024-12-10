using System.Linq;
using System.Reflection;
using AutoMapper;

namespace FclEx.Abp;

public static class AbpAutoMapperOptionsExtensions
{
    public static void AddMaps(this AbpAutoMapperOptions options, Assembly assembly, bool validate = false)
    {
        options.Configurators.Add(context => context.MapperConfiguration.AddMaps(assembly));
        if (validate)
        {
            var profileTypes = assembly.DefinedTypes
                .Where(type => typeof(Profile).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericType);

            foreach (var profileType in profileTypes)
            {
                options.ValidatingProfiles.Add(profileType);
            }
        }
    }
}