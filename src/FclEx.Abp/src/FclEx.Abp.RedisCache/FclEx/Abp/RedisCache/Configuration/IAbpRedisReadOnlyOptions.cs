using System;
using System.Text;

namespace FclEx.Abp.RedisCache.Configuration;

public interface IAbpRedisReadOnlyOptions
{
    bool SerializeStringAsRaw { get; }
}