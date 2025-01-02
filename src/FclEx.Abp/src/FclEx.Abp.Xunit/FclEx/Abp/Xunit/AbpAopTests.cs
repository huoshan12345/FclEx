using AspectCore.Extensions.DependencyInjection;

namespace FclEx.Abp.Xunit;

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