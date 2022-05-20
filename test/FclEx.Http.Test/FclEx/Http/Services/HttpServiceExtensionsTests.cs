using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Utils;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Http.Services
{
    public class HttpServiceExtensionsTests
    {
        public static string[] FileUrls { get; } =
        {
            "https://www.baidu.com/",
            "https://wx4.sinaimg.cn/mw690/006ODuFcly1fv1ey3f7m9j30tn0tnjsi.jpg",
            "https://www.cnblogs.com/kevinchoi/p/11716636.html#_label_h2_2",
            "https://www.baidu.com/s?wd=asdf&rsv_spt=1&rsv_iqid=0xb83160ce001946fb&issp=1&f=8&rsv_bp=1",
            "https://rm.api.weibo.com/2/remind/push_count.json?callback=STK_157208808117322"
        };

        public static IEnumerable<object[]> TestCasesOfDownload { get; } = FileUrls
            .Select(m => new Uri(m))
            .Select(m => (Url: m, FileName: Path.GetFileName(m.LocalPath)))
            .Select(m => (m.Url, m.FileName, Ext: Path.GetExtension(m.FileName)))
            .Select(m => new object[] { m.Url, m.FileName, m.Ext, m.FileName.TrimEnd(m.Ext) });

        [Theory]
        [InlineData("https://www.baidu.com/", "www_baidu_com.html")]
        [InlineData("https://wx4.sinaimg.cn/mw690/006ODuFcly1fv1ey3f7m9j30tn0tnjsi.jpg", "006ODuFcly1fv1ey3f7m9j30tn0tnjsi.jpg")]
        [InlineData("https://www.cnblogs.com/kevinchoi/p/11716636.html#_label_h2_2", "11716636.html")]
        [InlineData("https://www.baidu.com/s?wd=asdf&rsv_spt=1&rsv_iqid=0xb83160ce001946fb&issp=1&f=8&rsv_bp=1", "s.html")]
        [InlineData("https://rm.api.weibo.com/2/remind/push_count.json?callback=STK_157208808117322", "push_count.json")]
        public async Task DownloadAsync_Test(string uri, string fileName)
        {
            using var http = new HttpClientService();

            var (successful, file, exception, _) = await http.DownloadAsync(uri);

            AssertExt.True(successful, () => exception!.ToString());
            Assert.Equal(fileName, file.FileName);
            Assert.Equal(Path.GetExtension(fileName), file.FileExt);
            Assert.Equal(Path.GetFileNameWithoutExtension(fileName), file.FileNameWithoutExt);
        }

        [Fact(Skip = "no proxy")]
        public async Task DownloadAsync_WithProxy_403_Test()
        {
            using var http = new HttpClientService(proxy: GlobalConstants.DefaultProxy);

            const string url = "https://scontent-lga3-1.cdninstagram.com/v/t51.2885-15/e35/84633088_233319031038964_4686527252914001142_n.jpg" +
                               "?_nc_ht=scontent-lga3-1.cdninstagram.com&_nc_cat=104&_nc_ohc=rtLj-eg1T_sAX8YuTB5&oh=ee63e1a1e272f0826565ba4dc8f31174&oe=5E4D0FBF";

            var (successful, _, ex, _) = await http.DownloadAsync(url).DonotCapture();

            AssertExt.False(successful, () => ex!.ToString());
            Assert.True(ex.IsObjEx<HttpRes>(m => m.StatusCode == HttpStatusCode.Forbidden));
        }
    }
}
