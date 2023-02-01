using System;
using System.Threading.Tasks;
using FclEx.Extensions;
using FclEx.Helpers;
using Xunit;

namespace FclEx.Utils
{
    partial class OperateResultTests
    {
        [Fact]
        public async Task ExcuteAsync_Timeout_Test()
        {
            var (successful, exception, elapsed) = await Operate.ExcuteAsync(() => TaskHelper.Delay(10), TimeSpan.FromSeconds(1));
            Assert.False(successful);
            Assert.True(elapsed < TimeSpan.FromSeconds(2), elapsed.ToString());
            Assert.IsType<TimeoutException>(exception);
        }

        [Fact]
        public async Task ExcuteAsync_Timeout_Success_Test()
        {
            var (successful, result, _, elapsed) = await Operate.ExcuteAsync(async () =>
            {
                await TaskHelper.Delay(1).DonotCapture();
                return 1;
            }, TimeSpan.FromSeconds(10));
            Assert.True(successful);
            Assert.Equal(1, result);
            Assert.True(elapsed < TimeSpan.FromSeconds(1.1));
        }

        [Fact]
        public async Task ExcuteAsync_Timeout_SyncBody_Test()
        {
            var (successful, exception, elapsed) = await Operate.ExcuteAsync(() =>
            {
                ThreadHelper.Sleep(10);
                return Task.CompletedTask;
            }, TimeSpan.FromSeconds(1));
            Assert.False(successful);
            Assert.True(elapsed < TimeSpan.FromSeconds(1.1));
            Assert.IsType<TimeoutException>(exception);
        }

        [Fact]
        public async Task ExcuteAsync_Timeout_SyncBody_Success_Test()
        {
            var (successful, result, _, elapsed) = await Operate.ExcuteAsync(() =>
            {
                ThreadHelper.Sleep(1);
                return Task.FromResult(1);
            }, TimeSpan.FromSeconds(10));

            Assert.True(successful);
            Assert.Equal(1, result);
            Assert.True(elapsed < TimeSpan.FromSeconds(5), "Actual time is " + elapsed);
        }
    }
}
