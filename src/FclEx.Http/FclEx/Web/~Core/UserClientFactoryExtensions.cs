namespace FclEx.Web;

/// <summary>
/// Convenience overloads for creating user clients with proxy-backed HTTP services.
/// </summary>
public static class UserClientFactoryExtensions
{
    /// <summary>
    /// Creates a client using a new <see cref="HttpClientService"/> configured with an optional proxy.
    /// </summary>
    public static TClient Create<TClient, TAccount>(this IUserClientFactory<TClient, TAccount> factory, TAccount account, IWebProxy? proxy = null)
        where TClient : IUserClient<TAccount>
        where TAccount : IUserAccount
    {
        var fac = factory.ServiceProvider.GetService<ILoggerFactory>();
        var http = HttpClientService.Create(proxy, true, fac);
        return factory.Create(account, http);
    }

    /// <summary>
    /// Creates a non-generic user client using a new <see cref="HttpClientService"/> configured with an optional proxy.
    /// </summary>
    public static TClient Create<TClient>(this IUserClientFactory<TClient, IUserAccount> factory, IUserAccount account, IWebProxy? proxy = null)
        where TClient : IUserClient
    {
        return factory.Create<TClient, IUserAccount>(account, proxy);
    }

    /// <summary>
    /// Creates a client with a proxy constructed from a URI.
    /// </summary>
    public static TClient Create<TClient, TAccount>(this IUserClientFactory<TClient, TAccount> factory, TAccount account, Uri? proxy)
        where TClient : IUserClient<TAccount>
        where TAccount : IUserAccount
    {
        return factory.Create(account, WebProxyHelper.Create(proxy));
    }

    /// <summary>
    /// Creates a non-generic user client with a proxy constructed from a URI.
    /// </summary>
    public static TClient Create<TClient>(this IUserClientFactory<TClient, IUserAccount> factory, IUserAccount account, Uri? proxy = null)
        where TClient : IUserClient
    {
        return factory.Create<TClient, IUserAccount>(account, proxy);
    }

    /// <summary>
    /// Creates a client with a proxy constructed from a URI string.
    /// </summary>
    public static TClient Create<TClient, TAccount>(this IUserClientFactory<TClient, TAccount> factory, TAccount account, string? proxy)
        where TClient : IUserClient<TAccount>
        where TAccount : IUserAccount
    {
        return factory.Create(account, WebProxyHelper.Create(proxy));
    }

    /// <summary>
    /// Creates a non-generic user client with a proxy constructed from a URI string.
    /// </summary>
    public static TClient Create<TClient>(this IUserClientFactory<TClient, IUserAccount> factory, IUserAccount account, string? proxy = null)
        where TClient : IUserClient
    {
        return factory.Create<TClient, IUserAccount>(account, proxy);
    }
}
