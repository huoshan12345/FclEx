using FclEx.Abp.Xunit;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace FclEx.Abp.RedisCache
{
    public class AbpRedisUnreachableTests : AbpTests<AbpRedisTestModule>
    {
        protected override IConfigurationRoot BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.Unreachable.json", false, false)
                .Build();
        }

        public AbpRedisUnreachableTests(ITestOutputHelper output, Action<AbpTestsOptions>? action = null)
            : base(output, action)
        {
        }
    }
}
