using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using FclEx.Http.Core;
using FclEx.Http.Services;

namespace FclEx.Benchmark
{
    [MemoryDiagnoser]
    public class HttpServiceTest
    {
        private static readonly IHttpService _httpClientService = new HttpClientService();
        private static readonly IHttpService _httpClientExtService = new HttpClientExtService();
        private static readonly IHttpService _lightHttpService = new HttpWebRequestService();

        public static IEnumerable<object> Cases => new[]
        {
            "http://www.baidu.com/",
            "http://www.sina.com.cn/",
            "https://weibo.com/",
            "http://www.sohu.com/",
            "http://www.qq.com/",
        };
        // for single argument it's an IEnumerable of objects (object)
        // for multiple arguments it's an IEnumerable of array of objects (object[])

        [Benchmark]
        [ArgumentsSource(nameof(Cases))]
        public async ValueTask HttpClientService_Test(string url)
        {
            await _httpClientService.ExecuteAsync(HttpReq.Get(url)).DonotCapture();
        }

        [Benchmark]
        [ArgumentsSource(nameof(Cases))]
        public async ValueTask HttpClientExtService_Test(string url)
        {
            await _httpClientExtService.ExecuteAsync(HttpReq.Get(url)).DonotCapture();
        }

        [Benchmark]
        [ArgumentsSource(nameof(Cases))]
        public async ValueTask LightHttpService_Test(string url)
        {
            await _lightHttpService.ExecuteAsync(HttpReq.Get(url)).DonotCapture();
        }
    }
}
