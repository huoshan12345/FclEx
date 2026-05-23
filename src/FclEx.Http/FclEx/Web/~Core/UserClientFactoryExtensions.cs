namespace FclEx.Web;

public static class UserClientFactoryExtensions
{
    public static TClient Create<TClient, TAccount>(this IUserClientFactory<TClient, TAccount> factory, TAccount account, IWebProxy? proxy = null)
        where TClient : IUserClient<TAccount>
        where TAccount : IUserAccount
    {
        var fac = factory.ServiceProvider.GetService<ILoggerFactory>();
        var http = HttpClientService.Create(proxy, true, fac);
        return factory.Create(account, http);
    }

    public static TClient Create<TClient>(this IUserClientFactory<TClient, IUserAccount> factory, IUserAccount account, IWebProxy? proxy = null)
        where TClient : IUserClient
    {
        return factory.Create<TClient, IUserAccount>(account, proxy);
    }

    public static TClient Create<TClient, TAccount>(this IUserClientFactory<TClient, TAccount> factory, TAccount account, Uri? proxy)
        where TClient : IUserClient<TAccount>
        where TAccount : IUserAccount
    {
        return factory.Create(account, WebProxyHelper.Create(proxy));
    }

    public static TClient Create<TClient>(this IUserClientFactory<TClient, IUserAccount> factory, IUserAccount account, Uri? proxy = null)
        where TClient : IUserClient
    {
        return factory.Create<TClient, IUserAccount>(account, proxy);
    }

    public static TClient Create<TClient, TAccount>(this IUserClientFactory<TClient, TAccount> factory, TAccount account, string? proxy)
        where TClient : IUserClient<TAccount>
        where TAccount : IUserAccount
    {
        return factory.Create(account, WebProxyHelper.Create(proxy));
    }

    public static TClient Create<TClient>(this IUserClientFactory<TClient, IUserAccount> factory, IUserAccount account, string? proxy = null)
        where TClient : IUserClient
    {
        return factory.Create<TClient, IUserAccount>(account, proxy);
    }
}