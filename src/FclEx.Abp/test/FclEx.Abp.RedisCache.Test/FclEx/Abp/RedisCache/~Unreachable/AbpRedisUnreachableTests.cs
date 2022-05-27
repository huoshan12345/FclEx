using System;
using FclEx.Abp.RedisCache.Configuration;
using FclEx.Abp.Xunit;
using FclEx.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        public AbpRedisUnreachableTests(ITestOutputHelper output, Action<AbpTestsOptions> action = null)
            : base(output, action)
        {
        }
    }
}
