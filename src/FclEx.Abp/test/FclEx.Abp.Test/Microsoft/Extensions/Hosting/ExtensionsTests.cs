using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Abp;
using FclEx.Utils;
using LightInject;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Extensions.Hosting
{
    public class ExtensionsTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UseLightInject_Test(bool useAop)
        {
            var builder = new HostBuilder()
                .UseLightInject(useAop)
                .ConfigureServices((context, services) => services.AddApplication<AbpTestModule>());

            using var host = builder.Build();
            host.Services.UseAbp();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await host.RunAsync(cts.Token);
        }
    }
}
