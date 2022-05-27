using System;
using System.Diagnostics.CodeAnalysis;
using FclEx.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit.Abstractions;

// ReSharper disable StaticMemberInGenericType
namespace FclEx.Abp.Xunit
{
    public abstract class AbpStaticInitTests<TModule> : AbstractAbpTests<TModule>
        where TModule : AbpModule
    {
        protected static readonly object Locker = new();
        protected static bool IsReady { get; set; }
        public static IServiceProvider ServiceProvider { get; private set; } = default!;

        protected AbpStaticInitTests(ITestOutputHelper output, Action<AbpTestsOptions>? action = null)
            : base(output, action)
        {
            if (!IsReady)
            {
                lock (Locker)
                {
                    if (!IsReady)
                    {
                        ServiceProvider = InitApp();
                        IsReady = true;
                        return;
                    }
                }
            }
            var fac = ServiceProvider.GetRequiredService<ILoggerFactory>();
            fac.AddXunitTest(output, true);
        }
    }
}