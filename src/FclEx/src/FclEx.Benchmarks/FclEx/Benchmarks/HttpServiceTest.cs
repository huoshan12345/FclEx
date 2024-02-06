using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using FclEx.Extensions;
using FclEx.Http;

namespace FclEx.Benchmarks;

[MemoryDiagnoser]
public class HttpServiceTest
{
    private static readonly IHttpService _httpClientService = new HttpClientService();

    public static IEnumerable<object> Cases => new[]
    {
        "http://www.baidu.com/",
        "http://www.sina.com.cn/",
        "https://weibo.com/",
        "http://www.qq.com/",
    };
    // for single argument it's an IEnumerable of objects (object)
    // for multiple arguments it's an IEnumerable of array of objects (object[])

    [Benchmark]
    [ArgumentsSource(nameof(Cases))]
    public async ValueTask HttpClientService_Test(string url)
    {
        await _httpClientService.SendAsync(HttpRequest.Get(url)).IgnoreSyncContext();
    }
}