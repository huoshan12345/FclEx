using System;
using EasyCaching.Core.Configurations;

namespace FclEx.Abp.RedisCache.Configuration;

public class AbpRedisOptions : IAbpRedisReadOnlyOptions
{
    public bool UseMessagePack { get; set; } = true;
    public bool SerializeStringAsRaw { get; set; } = true;
    public List<CsRedisCoreConStr> ConStrs { get; } = [];

    private readonly List<IRedisCollectionConfigurator> _colConfigurators = [];
    public IReadOnlyList<IRedisCollectionConfigurator> ColConfigurators => _colConfigurators;

    public AbpRedisOptions ConfigureCollection(string name, Action<RedisCollectionOptions> action)
    {
        _colConfigurators.Add(new RedisCollectionConfigurator(name, action));
        return this;
    }

    public AbpRedisOptions ConfigureAllCollections(Action<RedisCollectionOptions> action)
    {
        _colConfigurators.Add(new RedisCollectionConfigurator(action));
        return this;
    }

    public void Deconstruct(
        out bool useMessagePack,
        out bool serializeStringAsRaw,
        out List<CsRedisCoreConStr> conStrs,
        out IReadOnlyList<IRedisCollectionConfigurator> configurators)
    {
        useMessagePack = UseMessagePack;
        serializeStringAsRaw = SerializeStringAsRaw;
        conStrs = ConStrs;
        configurators = ColConfigurators;
    }
}