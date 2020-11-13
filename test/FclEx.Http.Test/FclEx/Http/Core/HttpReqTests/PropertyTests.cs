using System.Threading.Tasks;
using FclEx.Http.Services;
using Xunit;

namespace FclEx.Http.Core.HttpReqTests
{
    public class PropertyTests
    {
        public static (string Url, string CharSet, string Keyword) CharSetTestCase = ("https://passport.weibo.com/visitor/visitor", "gb2312", "是否采集设备指纹");

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CharSet_Test(bool setProp)
        {
            using var http = new HttpClientService();
            var req = HttpReq.Get(CharSetTestCase.Url);
            if (setProp)
                req.CharSet(CharSetTestCase.CharSet);

            var res = await http.SendAsync(req)
                .ThrowIfError()
                .DonotCapture();
            Assert.Equal(setProp, res.ResponseString.Contains(CharSetTestCase.Keyword));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task FallbackCharSet_Test(bool setProp)
        {
            using var http = new HttpClientService();
            var req = HttpReq.Get(CharSetTestCase.Url);
            if (setProp)
                req.CharSet(CharSetTestCase.CharSet);

            var res = await http.SendAsync(req)
                .ThrowIfError()
                .DonotCapture();
            Assert.Equal(setProp, res.ResponseString.Contains(CharSetTestCase.Keyword));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DetectCharSetFromHtmlMeta_Test(bool setProp)
        {
            using var http = new HttpClientService();
            var req = HttpReq.Get(CharSetTestCase.Url);
            req.DetectCharSetFromHtmlMeta(setProp);

            var res = await http.SendAsync(req)
                .ThrowIfError()
                .DonotCapture();
            Assert.Equal(setProp, res.ResponseString.Contains(CharSetTestCase.Keyword));
        }
    }
}
