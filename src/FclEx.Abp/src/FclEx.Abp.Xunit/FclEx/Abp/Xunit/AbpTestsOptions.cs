using Microsoft.Extensions.DependencyInjection;

namespace FclEx.Abp.Xunit;

public class AbpTestsOptions
{
    public IServiceCollection Services { get; } = new ServiceCollection();
    public bool UseLightInject { get; set; } = false;
    public bool UseAop { get; set; } = true;
}