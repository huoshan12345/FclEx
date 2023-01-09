using System.Reflection;
using System.Text.RegularExpressions;
using EasyCaching.Core;
using EasyCaching.CSRedis;
using FclEx.Abp.RedisCache.Configuration;
using FclEx.Abp.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Abp.RedisCache
{
    public class TimeoutTests : AbpRedisUnreachableTests
    {
        public static FieldInfo FieldOfRedisOptions { get; }
            = typeof(DefaultCSRedisCachingProvider).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly Regex RegOfConTimeout = new(@"connectTimeout=(\d)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public TimeoutTests(ITestOutputHelper output, Action<AbpTestsOptions> action = null)
            : base(output, action)
        {
        }

        [Fact]
        public void SetTimeout_Test()
        {
            var (_, _, conStrs, _) = ServiceProvider.GetOptions<AbpRedisOptions>();
            var con = conStrs.Single();
            var provider = ServiceProvider.GetRequiredService<IEasyCachingProvider>();
            Assert.IsType<DefaultCSRedisCachingProvider>(provider);
            var csRedisProvider = (DefaultCSRedisCachingProvider)provider;
            var actualOptions = (RedisOptions)FieldOfRedisOptions.GetValue(csRedisProvider);
            Assert.Single(actualOptions!.DBConfig.ConnectionStrings);
            var str = actualOptions.DBConfig.ConnectionStrings.First();

            if (RegOfConTimeout.TryMatch(str, 1, out var value))
            {
                Assert.Equal(con.ConnectTimeout / 1000, int.Parse(value));
            }
            else
            {
                Assert.True(false);
            }
        }

        [Fact]
        public async Task WaitTimeout_Test()
        {
            var (_, _, conStrs, _) = ServiceProvider.GetOptions<AbpRedisOptions>();
            var con = conStrs.Single();
            var provider = ServiceProvider.GetRequiredService<IEasyCachingProvider>();
            var timeout = con.ConnectTimeout;
            var (successful, _, _, elapsed) = await Operate.ExcuteAsync(() => provider.GetAsync<string>("test"), TimeSpan.FromMilliseconds(timeout)).Unwrap();
            Assert.False(successful);
            Assert.True(elapsed.TotalMilliseconds < timeout + 1000, elapsed.TotalSeconds.ToString());
        }
    }
}
