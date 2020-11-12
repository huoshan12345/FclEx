using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FclEx.Helpers;
using Xunit;
using FclEx;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Logging
{
    public class LoggerFactoryExtensionsTests
    {
        public static IEnumerable<object[]> LogLevelCases { get; } =
            EnumHelper.GetValues<LogLevel>().Select(m => new object[] { m });

        [Theory]
        [MemberData(nameof(LogLevelCases))]
        public void SetMinimumLevel_Test(LogLevel logLevel)
        {
            var services = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var fac = services.GetRequiredService<ILoggerFactory>();
            fac.SetMinimumLevel(logLevel);

            var options = (LoggerFilterOptions)fac.GetType().InvokeMember(
                name: "_filterOptions",
                invokeAttr: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                binder: null,
                target: fac,
                args: null);

            Assert.Equal(logLevel, options?.MinLevel);
        }
    }
}