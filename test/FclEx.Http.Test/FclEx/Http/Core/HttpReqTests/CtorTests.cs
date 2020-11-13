using System.Collections.Generic;
using System.Linq;
using FclEx.Helpers;
using Xunit;

namespace FclEx.Http.Core.HttpReqTests
{
    public class CtorTests
    {
        public static string[] Urls { get; } =
        {
            "https://www.cnblogs.com/armfly/p/9378170.html",
            "/parent/change-old-passwd"
        };

        public static HttpMethodType[] Methods { get; } = EnumHelper.GetValues<HttpMethodType>();

        public static IEnumerable<object[]> CtorCases { get; } =
            Urls.SelectMany(m => Methods, (u, m) => new object[] { u, m });

        [Theory]
        [MemberData(nameof(CtorCases))]
        public void TestCtor(string url, HttpMethodType method)
        {
            var req = new HttpReq(url, method);
            req.Host("localhost");
            var realUrl = req.GetUrl();
        }

        [Fact]
        public void Ctor_WithUserInfo()
        {
            var req = HttpReq.Get("http://lijing:lijing@captcha.mooncatling.fun/api/captcha/save");
            Assert.True(req.HeaderMap.TryGetValue("Authorization", out var auth));
            Assert.Equal("Basic bGlqaW5nOmxpamluZw==", auth);
        }
    }
}
