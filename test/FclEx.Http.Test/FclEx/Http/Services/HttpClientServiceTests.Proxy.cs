using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using Xunit;

namespace FclEx.Http.Services
{
    partial class HttpClientServiceTests
    {
        public static IWebProxyExt[] ProxyList { get; } =
        {
            GlobalConstants.DefaultProxy
        };

        public static string[] Urls { get; } =
        {
            "https://www.google.com/",
            "https://www.instagram.com/",
            "https://www.limetorrents.com/"
        };

        public static IEnumerable<object[]> Cases { get; } = ProxyList.SelectMany(m => Urls, (x, y) => new object[] { x, y });

        [Theory(Skip = "no proxy")]
        [MemberData(nameof(Cases))]
        public async Task SendAsync_WithProxy_Success(IWebProxyExt proxy, string url)
        {
            var service = new HttpClientService(true, proxy);
            var res = await service.SendAsync(HttpReq.Get(url).ConnectTimeout(TimeSpan.FromSeconds(5)));
            AssertExt.False(res.HasError, () => res.Exception!.ToString());
        }
    }
}
