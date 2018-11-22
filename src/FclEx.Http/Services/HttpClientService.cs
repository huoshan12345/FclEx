using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SocksSharp;
using SocksSharp.Proxy;

namespace FclEx.Http.Services
{
    public sealed class HttpClientService : AbstractHttpClientService
    {
        public static TimerLazy<HttpClientService> Default { get; } = new TimerLazy<HttpClientService>(() =>
                new HttpClientService(null, false),
                LazyThreadSafetyMode.ExecutionAndPublication,
                TimeSpan.FromMinutes(2));

        private HttpClient _httpClient;
        private HttpMessageHandler _handler;

        private static HttpClientHandler CreateDefaultHandler(IWebProxyExt proxy = null)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false,
                MaxConnectionsPerServer = 64,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            if (proxy != null)
            {
                handler.UseProxy = true;
                handler.Proxy = proxy;
            }
            else
            {
                handler.UseProxy = false;
                handler.Proxy = null;
            }
            return handler;
        }

        private void SetHttpClient()
        {
            Check.NotNull(_handler, nameof(_handler));
            var httpClient = new HttpClient(_handler, false);
            httpClient.DefaultRequestHeaders.Add(HttpConstants.UserAgent, HttpConstants.DefaultUserAgent);
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _httpClient = httpClient;
        }

        protected override void SetProxy(IWebProxyExt proxy)
        {
            proxy = proxy ?? WebProxyExt.None;
            if (Equals(_webProxy, proxy)) return;

            switch (proxy.Type)
            {
                case ProxyType.None:
                case ProxyType.Http:
                case ProxyType.Https:
                {
                    _handler?.Dispose();
                    _handler = CreateDefaultHandler(_webProxy);
                    break;
                }
                case ProxyType.Socks5:
                {
                    _handler?.Dispose();
                    _handler = new ProxyClientHandler<Socks5>(new ProxySettings
                    {
                        Port = proxy.Port,
                        Host = proxy.Host,
                        Credentials = proxy.Credentials as NetworkCredential
                    });
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(proxy.Type), proxy.Type, null);
            }
            _webProxy = proxy;
            _httpClient?.Dispose();
            SetHttpClient();
        }

        public override ValueTask<HttpRes> ExecuteAsync(HttpReq httpReq, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return ExecuteAsync(_httpClient, httpReq, token);
        }

        public HttpClientService(
            IWebProxyExt proxy = null,
            bool useCookie = true)
            : base(useCookie, proxy)
        {
            _handler = CreateDefaultHandler(proxy);
            SetHttpClient();
        }

        public HttpClientService(
            HttpMessageHandler handler,
            bool useCookie,
            IWebProxyExt proxy = null,
            ILoggerFactory loggerFactory = null)
            : base(useCookie, proxy, loggerFactory)
        {
            _handler = Check.NotNull(handler, nameof(handler));
            SetHttpClient();
        }

        public override void Dispose()
        {
            _handler.Dispose();
            _httpClient.Dispose();
        }
    }
}
