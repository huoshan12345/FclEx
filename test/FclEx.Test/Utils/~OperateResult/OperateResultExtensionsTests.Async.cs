using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Utils;
using Xunit;

namespace FclEx.Test.Utils
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
