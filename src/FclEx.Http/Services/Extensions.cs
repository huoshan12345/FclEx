using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Http.HttpClientExt;
using FclEx.Http.Proxy;
using Microsoft.Extensions.DependencyInjection;

namespace FclEx.Http.Services
{
    public static class Extensions
    {
        public static IHttpService Create(
            this IHttpServiceFactory factory,
            HttpServiceType type,
            Uri proxy,
            bool useCookie = true)
        {
            return factory.Create(type, useCookie, WebProxyExt.Create(proxy));
        }

        public static IHttpService Create(
            this IHttpServiceFactory factory,
            HttpServiceType type,
            string proxy,
            bool useCookie = true)
        {
            return factory.Create(type, useCookie, WebProxyExt.Create(proxy));
        }

        public static IServiceCollection AddHttpService(this IServiceCollection services)
        {
            return services.AddHttpClientExt()
                .AddSingleton<IHttpServiceFactory, HttpServiceFactory>()
                .AddSingleton<HttpClientExtServiceFactory>();
        }
    }
}
