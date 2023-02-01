using System;
using System.Collections.Generic;
using EasyCaching.Core.Configurations;

namespace FclEx.Abp.RedisCache.Configuration
{
    public class AbpRedisOptions : IAbpRedisReadOnlyOptions
    {
        public bool UseMessagePack { get; set; } = true;
        public bool SerializeStringAsRaw { get; set; } = true;
        public List<CsRedisCoreConStr> ConStrs { get; } = new();

        private readonly List<IRedisColConfigurator> _configurators = new();
        public IReadOnlyList<IRedisColConfigurator> Configurators => _configurators;

        public AbpRedisOptions Configure(string name, Action<RedisColOptions> action)
        {
            _configurators.Add(new RedisColConfigurator(name, action));
            return this;
        }

        public AbpRedisOptions ConfigureAll(Action<RedisColOptions> action)
        {
            _configurators.Add(new RedisColConfigurator(action));
            return this;
        }

        public void Deconstruct(
            out bool useMessagePack,
            out bool serializeStringAsRaw,
            out List<CsRedisCoreConStr> conStrs,
            out IReadOnlyList<IRedisColConfigurator> configurators)
        {
            useMessagePack = UseMessagePack;
            serializeStringAsRaw = SerializeStringAsRaw;
            conStrs = ConStrs;
            configurators = Configurators;
        }
    }
}
