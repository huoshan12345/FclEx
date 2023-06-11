namespace FclEx.Web;

public static class UserClientFactoryExtensions
{
    public static TClient Create<TClient>(this IUserClientFactory<TClient> factory, IUserAccount account, IWebProxy proxy)
        where TClient : IUserClient
    {
        var http = new HttpClientService(true, proxy, factory.ServiceProvider.GetService<ILoggerFactory>());
        return factory.Create(account, http);
    }

    public static TClient Create<TClient>(this IUserClientFactory<TClient> factory, IUserAccount account, string? proxy) where TClient : IUserClient
    {
        return factory.Create(account, WebProxyHelper.Create(proxy));
    }
}