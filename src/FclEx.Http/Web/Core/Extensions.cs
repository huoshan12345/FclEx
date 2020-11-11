using System.Diagnostics.CodeAnalysis;
using FclEx.Http.Proxy;
using FclEx.Http.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FclEx.Web.Core
{
    public static class Extensions
    {
        public static TClient Create<TClient, TAccount>(this IUserClientFactory<TClient, TAccount> factory, [DisallowNull] TAccount account, IWebProxyExt proxy)
            where TClient : IUserClient, IHasAccount<TAccount>
        {
            var http = new HttpClientService(true, proxy, factory.ServiceProvider.GetService<ILoggerFactory>());
            return factory.Create(account, http);
        }

        public static TClient Create<TClient, TAccount>(this IUserClientFactory<TClient, TAccount> factory, [DisallowNull] TAccount account, string proxy)
            where TClient : IUserClient, IHasAccount<TAccount>
        {
            return factory.Create(account, WebProxyExt.Create(proxy));
        }
    }
}
