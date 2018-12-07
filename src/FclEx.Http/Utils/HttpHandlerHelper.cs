using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using FclEx.Http.Proxy;

namespace FclEx.Http.Utils
{
    public static class HttpHandlerHelper
    {
        public static HttpMessageHandler Create(IWebProxyExt proxy)
        {
            switch (proxy.Type)
            {
                case ProxyType.None: return CreateDefaultHandler(null);

                case ProxyType.Http:
                case ProxyType.Https:
                    return CreateDefaultHandler(proxy);

                case ProxyType.Socks5:
                    //return new ProxyClientHandler<Socks5>(new ProxySettings
                    //{
                    //    Port = proxy.Port,
                    //    Host = proxy.Host,
                    //    Credentials = proxy.Credentials as NetworkCredential
                    //});
                default:
                    throw new NotSupportedException();
            }
        }

        private static HttpClientHandler CreateDefaultHandler(IWebProxy proxy)
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
    }
}
