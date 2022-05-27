using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Abp.RedisCache.Configuration
{
    public interface IAbpRedisReadOnlyOptions
    {
        bool SerializeStringAsRaw { get; }
    }
}
