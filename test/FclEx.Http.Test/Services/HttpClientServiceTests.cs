using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using FclEx.Http.Services;
using FclEx.Utils;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Http.Test.Services
{
    public partial class HttpClientServiceTests
    {
        private readonly ITestOutputHelper _output;

        public HttpClientServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("http://localhost:1080")]
        public void Constructor_Test(string proxy)
        {
            var http = new HttpClientService(proxy: WebProxyExt.Create(proxy));
            Assert.Equal(WebProxyExt.Create(proxy), http.WebProxy);
        }

        [Fact]
        public async Task Tests()
        {
            var service = new HttpClientService(false);
            for (var i = 0; i < 5; i++)
            {
                var res = await HttpReq.Get("http://www.baidu.com")
                    .SendAsync(service);

                if (res.HasError)
                    _output.WriteLine(res.Exception.ToString());

                Assert.False(res.HasError);
            }
        }

        [Fact]
        public async Task ReLazyTests()
        {
            var service = new ReLazy<HttpClientService>(() => new HttpClientService(false),
                LazyThreadSafetyMode.ExecutionAndPublication);
            
            var first = service.Value;
            var last = service.Value;
            for (var i = 0; i < 5; i++)
            {
                last = service.Value;
                var res = await HttpReq.Get("http://www.baidu.com")
                    .SendAsync(last);

                if (res.HasError)
                    _output.WriteLine(res.Exception.ToString());

                Assert.False(res.HasError);
                service.Recreate();
            }
            Assert.NotEqual(first, last);
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
                var res = await HttpReq.Get("https://www.baidu.com")
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
