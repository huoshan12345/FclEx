using System;
using System.Threading.Tasks;
using FclEx.Extensions;
using Xunit;

namespace FclEx.Utils
{
    public class OperateResultExtensionsTests
    {
        [Fact]
        public async Task TaskOfOperateResult_Ok_Action_TimeSpan()
        {
            var elapsed = TimeSpan.FromHours(1);
            TimeSpan timeSpan = default;
            var result = await Operate.CreateSuccess(elapsed)
                .ToTask()
                .Ok((_, t) => timeSpan = t);

            Assert.True(result.Successful);
            Assert.Equal(elapsed, timeSpan);
        }
    }
}
