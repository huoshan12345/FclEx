namespace FclEx.Web;

public static class UserClientFactoryExtensions
{
    public static TClient Create<TClient>(this IUserClientFactory<TClient> factory, IUserAccount account, IWebProxy? proxy = null)
        where TClient : IUserClient
    {
        var fac = factory.ServiceProvider.GetService<ILoggerFactory>();
        var http = HttpClientService.Create(proxy, true, fac);
        return factory.Create(account, http);
    }

    public static TClient Create<TClient>(this IUserClientFactory<TClient> factory, IUserAccount account, Uri? proxy) where TClient : IUserClient
    {
        return factory.Create(account, WebProxyHelper.Create(proxy));
    }

    public static TClient Create<TClient>(this IUserClientFactory<TClient> factory, IUserAccount account, string? proxy) where TClient : IUserClient
    {
        return factory.Create(account, WebProxyHelper.Create(proxy));
    }
}