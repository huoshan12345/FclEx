using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using FclEx.Http.Utils;
using FclEx.Utils;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Services
{
    public sealed class HttpClientService : AbstractHttpClientService
    {
        public static TimerLazy<HttpClientService> Default { get; } = new TimerLazy<HttpClientService>(() =>
                new HttpClientService(false, null, null),
                LazyThreadSafetyMode.ExecutionAndPublication,
                TimeSpan.FromMinutes(2), false);

        private static readonly TimerLazy<HttpClient> _httpClient =
            new TimerLazy<HttpClient>(() => CreateHttpClient(_funcOfHandler()),
            LazyThreadSafetyMode.ExecutionAndPublication,
            TimeSpan.FromMinutes(2), false);

        private static Func<HttpMessageHandler> _funcOfHandler;

        private static HttpMessageHandler CreateHandler(IWebProxyExt proxy)
        {
            return HttpHandlerHelper.Create(proxy);
        }

        private static HttpClient CreateHttpClient(HttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler, false) { Timeout = Timeout.InfiniteTimeSpan };
            httpClient.DefaultRequestHeaders.Add(HttpConstants.UserAgent, HttpConstants.DefaultUserAgent);
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            return httpClient;
        }

        protected override void SetProxy(IWebProxyExt proxy)
        {
            proxy = proxy ?? WebProxyExt.None;
            if (Equals(WebProxy, proxy)) return;
            _webProxy = proxy;
            _httpClient.Recreate();
        }

        protected override Task ExecuteAsyncInternal(HttpReq httpReq, HttpRes httpRes, CancellationToken token)
        {
            return ExecuteAsyncInternal(_httpClient.Value, httpReq, httpRes, token);
        }

        public HttpClientService(
            bool useCookie = true,
            IWebProxyExt proxy = null,
            ILoggerFactory loggerFactory = null)
            : base(useCookie, proxy, loggerFactory)
        {
            _funcOfHandler = () => CreateHandler(WebProxy);
        }

        public override void Dispose()
        {
        }
    }
}
