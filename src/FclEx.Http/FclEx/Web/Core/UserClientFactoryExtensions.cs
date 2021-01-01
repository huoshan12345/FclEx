using FclEx.Http.Proxy;
using FclEx.Http.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FclEx.Web.Core
{
    public static class UserClientFactoryExtensions
    {
        public static TClient Create<TClient>(this IUserClientFactory<TClient> factory, IUserAccount account, IWebProxyExt proxy)
            where TClient : IUserClient
        {
            var http = new HttpClientService(true, proxy, factory.ServiceProvider.GetService<ILoggerFactory>());
            return factory.Create(account, http);
        }

        public static TClient Create<TClient>(this IUserClientFactory<TClient> factory, IUserAccount account, string proxy) where TClient : IUserClient
        {
            return factory.Create(account, WebProxyExt.Create(proxy));
        }
    }
}
