using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Abp.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAbpDistributedCache(this IServiceCollection services)
        {
            services.AddSingleton<IDistributedCache, AbpDistributedCache>();
            return services;
        }
    }
}
