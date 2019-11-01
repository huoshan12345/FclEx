using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using FclEx.Http.Services;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Http.Test.Services
{
    public partial class HttpClientServiceTests
    {
        public static IWebProxyExt[] ProxyList { get; } =
        {
            GlobalConstants.DefaultProxy
            // new WebProxyExt(ProxyType.Https, "127.0.0.1", 1080),
            // new WebProxyExt(ProxyType.Socks5, "127.0.0.1", 1080),
        };

        public static string[] Urls { get; } =
        {
            "https://www.google.com/",
            "https://www.baidu.com/",
        };

        public static IEnumerable<object[]> Cases { get; } = ProxyList.SelectMany(m => Urls, (x, y) => new object[] { x, y });

        [Theory]
        [MemberData(nameof(Cases))]
        public async Task Test(IWebProxyExt proxy, string url)
        {
            var service = new HttpClientService(true, proxy);
            var res = await service.SendAsync(HttpReq.Get(url).Timeout(10 * 1000));
            if (res.HasError)
                _output.WriteLine(res.Exception.ToString());
            Assert.False(res.HasError);
        }
    }
}
