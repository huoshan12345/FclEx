using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FclEx.Extensions;
using FclEx.Http.Core;
using FclEx.Utils;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Http.Services
{
    public class HttpServiceExtensionsTests
    {
        [Theory]
        [InlineData("https://www.baidu.com/", "www_baidu_com.html")]
        [InlineData("https://www.cnblogs.com/kevinchoi/p/11716636.html#_label_h2_2", "11716636.html")]
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
