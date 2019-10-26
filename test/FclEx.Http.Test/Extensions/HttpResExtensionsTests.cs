using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Http.Core;
using FclEx.Utils;
using Xunit;

namespace FclEx.Http.Test.Extensions
{
    public class HttpResExtensionsTests
    {
        private static async ValueTask SuccessRequest()
        {
            await HttpReq.Get("https://www.baidu.com")
                .SendAsync()
                .ThrowIfError();
            await TaskHelper.Delay(3);
        }

        private static async Task SuccessRequestWrap()
        {
            await HttpReq.Get("https://www.baidu.com")
                .SendAsync()
                .ThrowIfError();
            await TaskHelper.Delay(3);
        }

        private static async ValueTask TimeoutRequest()
        {
            await HttpReq.Get("https://www.google.com")
                .SendAsync()
                .ThrowIfError();
        }

        private static async Task TimeoutRequestWrap()
        {
            await HttpReq.Get("https://www.google.com")
                .SendAsync()
                .ThrowIfError();
        }

        [Fact]
        public async Task ThrowIfError_ValueTask_Test()
        {
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await HttpReq.Get("http://localhost:9999")
                .SendAsync()
                .ThrowIfError());
        }

        [Fact]
        public async Task ThrowIfError_ValueTask_Excute_Test()
        {
            var flag = false;
            var r = await OperateResult.ExcuteAsync(() => TimeoutRequestWrap())
                .Ok(_ => flag = true)
                .Error(e =>
                {
                    flag = true;
                    Assert.IsType<OperationCanceledException>(e);
                });
            Assert.True(flag);
            Assert.False(r.Successful);
        }

        [Fact]
        public async Task Excute_Test()
        {
            var flag = false;
            var r = await OperateResult.ExcuteAsync(() => SuccessRequestWrap())
                .Ok(_ => flag = true)
                .Error(e =>
                {
                    flag = true;
                    Assert.IsType<OperationCanceledException>(e);
                });
            Assert.True(flag);
            Assert.True(r.Successful);
        }
    }
}
