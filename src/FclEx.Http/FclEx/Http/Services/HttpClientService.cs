using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using FclEx.Utils;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Services
{
    public sealed class HttpClientService : AbstractHttpClientService
    {
        private static readonly TimerLazy<HttpClientService> _default = new(() => new HttpClientService(false, null, null), TimeSpan.FromMinutes(2));

        public static HttpClientService Default => _default.Value;

        private readonly TimerLazy<HttpClient> _httpClient;

        private static HttpClient CreateHttpClient(HttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler, disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };
            httpClient.DefaultRequestHeaders.Add(HttpKnownHeaderNames.UserAgent, HttpConstants.DefaultUserAgent);
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            return httpClient;
        }

        protected override void SetProxy(IWebProxyExt proxy)
        {
            proxy ??= WebProxyExt.None;
            if (Equals(_webProxy, proxy)) return;
            _webProxy = proxy;
            _httpClient?.Recreate();
        }

        protected override Task ExecuteAsyncInternal(HttpReq httpReq, HttpRes httpRes, CancellationToken token)
        {
            return ExecuteAsyncInternal(_httpClient.Value, httpReq, httpRes, token);
        }

        public HttpClientService(bool useCookie = true, IWebProxyExt? proxy = null, ILoggerFactory? loggerFactory = null)
            : base(useCookie, proxy, loggerFactory)
        {
            _httpClient = new TimerLazy<HttpClient>(() => CreateHttpClient(HttpHandlerHelper.Create(WebProxy)), TimeSpan.FromMinutes(2));
        }

        public override void Dispose()
        {
        }
    }
}
