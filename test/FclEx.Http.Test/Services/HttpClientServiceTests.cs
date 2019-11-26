using System;
using System.Collections.Generic;
using System.Linq;
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
            using var service = new HttpClientService(false);
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
            using var service = new ReLazy<HttpClientService>(() => new HttpClientService(false));

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
            using var service = new TimerLazy<HttpClientService>(() => new HttpClientService(false),
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddCookie_Test(bool useCookie)
        {
            var uri = new Uri("https://www.instagram.com/");
            var cookies = GlobalConstants.SimpleCookies;
            using var service = new HttpClientService(useCookie);
            foreach (var cookie in cookies.Select(m => m.ToCookie()))
                service.AddCookie(cookie, uri);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddCookie_NullUri_Test(bool useCookie)
        {
            var cookies = GlobalConstants.SimpleCookies;
            using var service = new HttpClientService(useCookie);
            foreach (var cookie in cookies.Select(m => m.ToCookie()))
                service.AddCookie(cookie, null);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetAllCookies_Test(bool useCookie)
        {
            var cookies = GlobalConstants.SimpleCookies.Select(m => m.ToCookie()).ToDictionary(m => m.Name);
            using var service = new HttpClientService(useCookie);
            foreach (var cookie in cookies.Values)
                service.AddCookie(cookie, null);

            var actualCookies = service.GetAllCookies();
            if (useCookie)
            {
                Assert.Equal(cookies.Count, actualCookies.Count);
                foreach (var actualCookie in actualCookies)
                {
                    Assert.True(cookies.TryGetValue(actualCookie.Name, out var cookie));
                    Assert.Equal(cookie.Value, actualCookie.Value);
                    Assert.Equal(cookie.Domain, actualCookie.Domain);
                }
            }
            else
            {
                Assert.Empty(actualCookies);
            }
        }
    }
}
