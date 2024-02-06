using System;
using System.Collections.Generic;
using System.Text;
using EasyCaching.Core.Serialization;
using EasyCaching.CSRedis;
using Microsoft.Extensions.Logging;

namespace FclEx.Abp.RedisCache;

public sealed class AbpCsRedisCachingProvider : DefaultCSRedisCachingProvider
{
    public AbpCsRedisCachingProvider(string name, 
        IEnumerable<EasyCachingCSRedisClient> clients, 
        IEnumerable<IEasyCachingSerializer> serializers, 
        RedisOptions options, 
        ILoggerFactory? loggerFactory = null) 
        : base(name, clients, serializers, options, loggerFactory)
    {
    }
}