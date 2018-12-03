using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Http.Proxy;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Services
{
    public class HttpServiceFactory : IHttpServiceFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public HttpServiceFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IHttpService Create(HttpServiceType type, bool useCookie = true, IWebProxyExt proxy = null)
        {
            switch (type)
            {
                case HttpServiceType.HttpClient:
                    return new HttpClientService(useCookie, proxy, _loggerFactory);
                case HttpServiceType.HttpClientExt:
                    return new HttpClientExtService(useCookie, proxy, _loggerFactory);
                case HttpServiceType.HttpWebRequest:
                    return new HttpWebRequestService(useCookie, proxy, _loggerFactory);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
