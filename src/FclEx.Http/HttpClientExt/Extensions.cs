using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Http.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FclEx.Http.HttpClientExt
{
    public static class Extensions
    {
        public static IServiceCollection AddHttpClientExt(this IServiceCollection services)
        {
            return services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>()
                .AddSingleton<IHttpMessageHandlerFactory, DefaultHttpMessageHandlerFactory>();
        }
    }
}
