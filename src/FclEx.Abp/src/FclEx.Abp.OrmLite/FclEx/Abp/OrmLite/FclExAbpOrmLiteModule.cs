using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using FclEx.Abp.Domain.Entities.Interfaces;
using FclEx.Abp.Orm;
using FclEx.Extensions;
using FclEx.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Reflection;

namespace FclEx.Abp.OrmLite
{
    [DependsOn(typeof(FclExAbpModule))]
    public class FclExAbpOrmLiteModule : AbpModule
    {
        private static readonly Initializer _initializer = new();

        static FclExAbpOrmLiteModule()
        {
            JsConfig.Reset(); // To initialize ServiceStack cache, prevent it initializing at an unexpected time.
            OrmLiteConfig.StripUpperInLike = true; // NOTE, if it is false, query contains "like" will be very slow.
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            _initializer.Init(() => context.ServiceProvider.AddOrmLiteAttributeForAllEntityTypes());

            var logger = context.ServiceProvider.CreateLogger(GetType());
            var resolver = context.ServiceProvider.GetRequiredService<IOrmLiteConStrResolver>();
            var cons = resolver.GetConStrs();
            if (cons.Any())
            {
                var defaultCon = cons.First();
                OrmLiteConfig.DialectProvider = defaultCon.Provider;
                foreach (var con in cons)
                {
                    logger.LogInformation($"A database connection named {con.Name} with provider of {con.Provider.GetType().ShortName()} has been registered");
                }
            }
            else
            {
                logger.LogWarning("There is no database connection to be registered");
            }
        }

    }
}
