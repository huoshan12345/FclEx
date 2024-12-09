using System;
using Volo.Abp.Modularity;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace FclEx.Abp.Xunit;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public abstract class AbpAopTests<TModule> : AbpTests<TModule>
    where TModule : AbpModule
{
    protected override LogLevel LogLevel => LogLevel.Debug;

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