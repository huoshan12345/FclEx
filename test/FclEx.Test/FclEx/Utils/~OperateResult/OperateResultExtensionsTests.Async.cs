using System;
using System.Threading.Tasks;
using Xunit;

namespace FclEx.Utils
{
    partial class OperateResultExtensionsTests
    {
        [Fact]
        public async Task TaskOfOperateResult_Ok_Action_TimeSpan()
        {
            var elapsed = TimeSpan.FromHours(1);
            TimeSpan timeSpan = default;
            var result = await OperateResult.CreateSuccess(elapsed)
                .ToTask()
                .Ok(t => timeSpan = t);

            Assert.True(result.Successful);
            Assert.Equal(elapsed, timeSpan);
        }
    }
}
