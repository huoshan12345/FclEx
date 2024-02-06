using System;
using FclEx.Abp.Caching.Configuration;

namespace FclEx.Abp.RedisCache.Configuration;

public class RedisColOptions
{
    internal RedisColOptions(string name)
    {
        Name = name;
    }

    public bool UseGlobalPrefix { get; set; } = false;
    public string Name { get; }
    public TimeSpan? DefaultExpiration { get; set; }
}