using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Extensions;
using FclEx.Utils;
using Xunit;

namespace FclEx.Http.Core.HttpReqTests
{
    public class SendTests
    {
 
        public static IList<string> Urls => new[]
        {
            "http://www.baidu.com/",
            "http://www.sina.com.cn/",
            "http://www.sohu.com/",
            "http://www.qq.com/",
        };

        public static IEnumerable<object[]> Cases => Urls
            .Select(m => new object[] { m });

        [Theory]
        [MemberData(nameof(Cases))]
        public async Task Get_Test(string url)
        {
            var res = await HttpReq.Get(url)
                .SendAsync()
                .DonotCapture();
            res.ThrowIfError();
        }

        [Fact]
        public async Task Form_Test()
        {
            var random = new Random(1024);
            var expected = Enumerable.Range(1, 3).ToDictionary(m => m.ToString(), m => random.NextString(5));
            var res = await HttpReq.Form(UrlUtil.Combine(GlobalConstants.TestUrl, "/api/post"))
                .AddData(expected)
                .SendAsync()
                .ThrowIfError()
                .DonotCapture();
            Assert.False(res.HasError);
            var body = res.ResponseString.ToJToken()["body"];
            Assert.NotNull(body);
            var actual = body.ToObject<Dictionary<string, string>>();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Json_Test()
        {
            var list = Enumerable.Range(1, 10).ToList();
            var res = await HttpReq.Json(UrlUtil.Combine(GlobalConstants.TestUrl, "/api/post"))
                .JsonBody(list)
                .SendAsync()
                .ThrowIfError()
                .DonotCapture();
            Assert.False(res.HasError);
            var body = res.ResponseString.ToJToken()["body"];
            Assert.NotNull(body);
            var actual = body.ToObject<List<int>>();
            Assert.True(list.SequenceEqual(actual!));
        }
    }
}
