using System;
using Volo.Abp.Modularity;
using Xunit.Abstractions;

namespace FclEx.Abp.Xunit;

public abstract class AbpTests<TModule> : AbstractAbpTests<TModule>
    where TModule : AbpModule
{
    protected AbpTests(ITestOutputHelper output, Action<AbpTestsOptions>? action = null)
        : base(output, action)
    {
        ServiceProvider = InitApp();
    }

    public IServiceProvider ServiceProvider { get; }
}