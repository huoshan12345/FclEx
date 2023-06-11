using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Abp.Caching.Configuration;

public static class Extensions
{
    public static AbpCacheOptions SetCacheExpireTime(this AbpCacheOptions configuration, string name, TimeSpan timeSpan)
    {
        configuration.Configure(name, o => o.DefaultExpiration = timeSpan);
        return configuration;
    }
}