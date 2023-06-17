using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Extensions.Logging
{
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public class ExtensionsTests
    {
        [Fact]
        public void IsNullOrNullLogger_Test()
        {
            Assert.True(NullLogger.Instance.IsNullOrNullLogger());

            Assert.True(NullLogger<int>.Instance.IsNullOrNullLogger());

            Assert.True(((ILogger?)null).IsNullOrNullLogger());

            Assert.True(((ILogger<int>?)null).IsNullOrNullLogger());

            var fac = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetRequiredService<ILoggerFactory>();

            Assert.False(fac.CreateLogger("test").IsNullOrNullLogger());

            Assert.False(fac.CreateLogger<ExtensionsTests>().IsNullOrNullLogger());
        }
    }
}
