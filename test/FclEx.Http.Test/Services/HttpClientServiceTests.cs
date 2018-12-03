using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Http.Test.Services
{
    public class HttpClientServiceTests
    {
        private readonly ITestOutputHelper _output;

        public HttpClientServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TimerLazyTests()
        {
            var service = new TimerLazy<HttpClientService>(() => new HttpClientService(false),
                LazyThreadSafetyMode.ExecutionAndPublication,
                TimeSpan.FromMilliseconds(100));

            var first = service.Value;
            var last = service.Value;
            for (var i = 0; i < 5; i++)
            {
                last = service.Value;
                var res = await HttpReq.Get("http://www.baidu.com")
                    .UserAgent(HttpConstants.DefaultUserAgent)
                    .SendAsync(last);
                if (res.HasError)
                    _output.WriteLine(res.Exception.ToString());

                Assert.False(res.HasError);
                await TaskHelper.DelayMilli(50);
            }
            Assert.NotEqual(first, last);
        }
    }
}
