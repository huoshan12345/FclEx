using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using FclEx.Http.Services;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Http.Test.ProxyTests
{
    public class HttpClientServiceProxyTests
    {
        private readonly ITestOutputHelper _output;

        public HttpClientServiceProxyTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public static IWebProxyExt[] ProxyList { get; } =
        {
            new WebProxyExt(ProxyType.Http, "127.0.0.1", 1080),
            new WebProxyExt(ProxyType.Https, "127.0.0.1", 1080),
            // new WebProxyExt(ProxyType.Socks5, "127.0.0.1", 1080),
        };

        public static IEnumerable<object[]> Cases { get; } = ProxyList.Select(m => new object[] { m });

        [Theory]
        [MemberData(nameof(Cases))]
        public async Task Test(IWebProxyExt proxy)
        {
            var service = new HttpClientService(true, proxy);
            var res = await service.SendAsync(HttpReq.Get("https://www.google.com/"));
            if (res.HasError)
                _output.WriteLine(res.Exception.ToString());
            Assert.False(res.HasError);
        }
    }
}
