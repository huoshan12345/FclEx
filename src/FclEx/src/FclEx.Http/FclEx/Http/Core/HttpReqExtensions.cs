using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FclEx.Http.Services;
using Polly;

namespace FclEx.Http.Core
{
    public static partial class HttpReqExtensions
    {
        public static Task<HttpRes> SendAsync(this HttpReq req, IHttpService? service = null, int retryTimes = 0, int delaySeconds = 0)
        {
            return (service ?? HttpClientService.Default).SendAsync(req, retryTimes, delaySeconds);
        }

        public static Task<HttpRes> SendAsync(this HttpReq req, IHttpService service, IAsyncPolicy policy)
        {
            return policy.ExecuteAsync(() => req.SendAsync(service));
        }
    }
}
