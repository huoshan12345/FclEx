using System;
using Xunit;

namespace FclEx.Utils
{
    public partial class OperateResultExtensionsTests
    {
        [Fact]
        public void OperateResult_Ok_Action_TimeSpan()
        {
            var elapsed = TimeSpan.FromHours(1);
            TimeSpan timeSpan = default;
            var result = OperateResult.CreateSuccess(elapsed)
                .Ok(t => timeSpan = t);

            Assert.True(result.Successful);
            Assert.Equal(elapsed, timeSpan);
        }
    }
}
