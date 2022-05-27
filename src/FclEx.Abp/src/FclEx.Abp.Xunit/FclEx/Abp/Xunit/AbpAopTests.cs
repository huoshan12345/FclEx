using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Xunit.Abstractions;

namespace FclEx.Abp.Xunit
{
    public abstract class AbpAopTests<TModule> : AbpTests<TModule>
        where TModule : AbpModule
    {
        protected AbpAopTests(ITestOutputHelper output, Action<IServiceCollection>? action = null)
            : base(output, o =>
            {
                o.UseLightInject = true;
                o.UseAop = true;
                action?.Invoke(o.Services);
            })
        {
        }
    }
}