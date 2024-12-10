using AspectCore.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace FclEx.Abp.Xunit;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public abstract class AbpAopTests<TModule> : AbpTests<TModule>
    where TModule : AbpModule
{
    protected override LogLevel LogLevel => LogLevel.Debug;

    protected AbpAopTests(ITestOutputHelper output) : base(output)
    {
    }

    protected override void Configure(AbpApplicationCreationOptions options, IConfigurationRoot configuration)
    {
        options.Services.AddAop();
    }
}