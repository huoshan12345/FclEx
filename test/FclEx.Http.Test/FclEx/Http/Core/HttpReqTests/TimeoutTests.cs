using System;
using System.Threading.Tasks;
using FclEx.Utils;
using Xunit;

namespace FclEx.Http.Core.HttpReqTests
{
    public class TimeoutTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public async Task TotalTimeout_Test(int timeout)
        {
            var req = HttpReq.Get("http://127.0.0.0")
                .TotalTimeout(TimeSpan.FromSeconds(timeout));

            var (successful, elapsed, _, exception) = await Operate.ExcuteAsync(async () => await req.SendAsync().ThrowIfError());
            Assert.False(successful);
            Assert.IsType<TaskCanceledException>(exception);
            var seconds = elapsed.TotalSeconds;
            Assert.True(seconds < timeout + 0.2);
            Assert.True(seconds > timeout - 0.2);
        }
    }
}
