using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FclEx.Actions;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;
using Xunit;

namespace FclEx.Http.Test.Actions
{
    public class HttpReqActionTests
    {
        [Fact]
        public async Task MutipleActions_Tests()
        {
            var uri = UrlUtil.Combine(GlobalConstants.TestUrl, "/api/post");
            using var http = HttpClientService.Default;
            var (successful, _, data, _) = await HttpReq.Json(uri).JsonBody(Enumerable.Range(1, 10).ToList())
                .ToAction(http)
                .ReadJson<List<int>>("body")
                .NextReq(m => HttpReq.Json(uri).JsonBody(m.Select(x => x.ToString()).ToDictionary(x => x, x => x + x)), http)
                .ReadJson<Dictionary<string, string>>("body")
                .ExecuteAsync()
                .DonotCapture();
            Assert.True(successful);
            var dic = Enumerable.Range(1, 10)
                .Select(x => x.ToString())
                .ToDictionary(x => x, x => x + x);
            Assert.Equal(dic, data);
        }
    }
}
