using FclEx.Abp.RabbitMQ.MsgRoles.Testers;
using FclEx.Abp.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Abp.RabbitMQ
{
    public class FclExAbpRabbitMqModuleTests : AbpTests<FclExAbpRabbitMqModuleTestsModule>
    {
        public FclExAbpRabbitMqModuleTests(ITestOutputHelper output, Action<AbpTestsOptions> action = null)
            : base(output, action)
        {
        }

        [Fact]
        public void Router_GetAndDispose_Test()
        {
            using var router = ServiceProvider.GetRequiredService<DiTestRouter>();
        }
    }
}
