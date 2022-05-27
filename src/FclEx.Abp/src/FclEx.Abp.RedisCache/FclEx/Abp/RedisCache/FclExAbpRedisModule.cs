using System.Collections.Generic;
using System.Linq;
using System.Text;

using EasyCaching.Core.Configurations;
using EasyCaching.Core.Serialization;
using EasyCaching.CSRedis;
using EasyCaching.Serialization.MessagePack;
using FclEx.Abp.Caching.Configuration;
using FclEx.Abp.RedisCache.Configuration;
using FclEx.Abp.Serializers;
using FclEx.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace FclEx.Abp.RedisCache
{
    [DependsOn(typeof(FclExAbpModule))]
    public class FclExAbpRedisModule : AbpModule
    {
        public const string DefaultJsonName = "json";
        public const string DefaultMsgPackName = "msgpack";
        public const string DefaultStringAsRawName = StringAsRawEasyCachingSerializer.DefaultName;

        private AbpRedisOptions? _effectiveRedisOptions;

        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IRedisColManager, RedisColManager>();
        }

        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
            var (useMessagePack, serializeStringAsRaw, conStrs, _) = (_effectiveRedisOptions = context.Services.GetOptions<AbpRedisOptions>());
            var serializerName = useMessagePack ? DefaultMsgPackName : DefaultJsonName;

            context.Services.AddEasyCaching(o =>
            {
                o.UseCSRedis(c =>
                {
                    c.SerializerName = serializeStringAsRaw ? DefaultStringAsRawName : serializerName;
                    c.DBConfig.ConnectionStrings = conStrs.Select(x => x.ToString()).ToList();
                });
                if (useMessagePack)
                    o.WithMessagePack(DefaultMsgPackName);
            });

            if (serializeStringAsRaw)
                context.Services.WrapFor<IEasyCachingSerializer>(m => new StringAsRawEasyCachingSerializer(m, name: DefaultStringAsRawName));
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            var provider = context.ServiceProvider;
            var logger = provider.CreateLogger(GetType());

            var conStrs = _effectiveRedisOptions?.ConStrs;
            if (conStrs?.Any() == true)
            {
                logger.LogInformation("Redis endpoints: " + conStrs.Select(m => $"{m.Host}:{m.Port}").JoinWith(","));
            }
            else
            {
                logger.LogWarning("No valid redis connection has been added.");
            }
        }
    }
}
