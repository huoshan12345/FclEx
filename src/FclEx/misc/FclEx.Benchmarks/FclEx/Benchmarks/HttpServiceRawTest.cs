using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Extensions;
using FclEx.Http;
using FclEx.Utils;

namespace FclEx.Benchmarks;

public class HttpServiceRawTest
{
    public static IList<string> Urls => new[]
    {
        "https://www.baidu.com/",
        "https://www.sina.com.cn/",
        "https://www.sohu.com/",
        "https://www.qq.com/",
        "https://www.163.com/",
        "http://www.ifeng.com/",
        "https://www.taobao.com/",
        "https://www.zhihu.com/",
    };

    public static IList<IHttpService> Services => new IHttpService[]
    {
        new HttpClientService()
    };

    public static async ValueTask RawTest(int rounds)
    {
        var reqs = Urls.Select(m => HttpRequest.Get(m)
                .ReadContent(false)
                .ReadAsBytes())
            .ToArray();

        foreach (var service in Services)
        {
            await RawTest(service, reqs, rounds).DonotCapture();
        }
    }

    public static async ValueTask RawTest(IHttpService service, IList<HttpRequest> reqs, int rounds)
    {
        var name = service.GetType().SimpleName();
        var before = GC.GetTotalMemory(true);
        var (success, ex, t) = await Operate.ExecuteAsync(async () =>
        {
            for (var i = 0; i < rounds; i++)
            {
                if (i % 100 == 0 && i > 0)
                {
                    Console.WriteLine($"[{name}]: Finished {i} Rounds");
                }

                var resList = await reqs.Select(m => service.SendAsync(m)).WhenAll().IgnoreSyncContext();
                resList.ForEach(m => m.ThrowIfError());

                //foreach (var req in reqs)
                //{
                //    var res = await service.ExecuteAsync(req).IgnoreSyncContext();
                //    res.ThrowIfError();
                //}
            }
        }).IgnoreSyncContext();
        var after = GC.GetTotalMemory(true);
        Console.WriteLine($"[{name}]: " +
                          $"Total Round: {rounds}, " +
                          $"Time: {t.TotalSeconds:f2}s, " +
                          $"Memory: {after - before}byte");
    }
}