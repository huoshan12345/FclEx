using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Services;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Http.Test.HttpReqTests
{
    public class SendTests
    {
        private readonly ITestOutputHelper _output;

        public SendTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public static IList<string> Urls => new[]
        {
            "http://www.baidu.com/",
            "http://www.sina.com.cn/",
            "https://weibo.com/",
            "http://www.sohu.com/",
            "http://www.qq.com/",
        };

        public static IList<IHttpService> Services => new IHttpService[]
        {
            new HttpClientService(),
            new HttpWebRequestService()
        };

        public static IEnumerable<object[]> Cases => Services
            .SelectMany(m => Urls, (i, j) => (Service: i, Url: j))
            .Select(m => new object[] { m.Service, m.Url });

        [Theory]
        [MemberData(nameof(Cases))]
        public async ValueTask ReqTest(IHttpService service, string url)
        {
            for (var i = 0; i < 3; i++)
            {
                var res = await service.SendAsync(HttpReq.Get(url)).DonotCapture();
                res.ThrowIfError();
            }
        }
    }
}
