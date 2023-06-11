using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AutoMapper;

namespace Volo.Abp.AutoMapper;

public static class Extensions
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