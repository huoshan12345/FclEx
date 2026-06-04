namespace FclEx.Web;

public static class ServiceProviderExtensions
{
    public static TClient CreateUserClient<TClient, TAccount>(this IServiceProvider provider, TAccount account, IHttpService? httpService = null)
        where TClient : IUserClient<TAccount>
        where TAccount : IUserAccount
    {
        return provider.GetRequiredService<IUserClientFactory<TClient, TAccount>>().Create(account, httpService);
    }

    public static TClient CreateUserClient<TClient>(this IServiceProvider provider, IUserAccount account, IHttpService? httpService = null)
        where TClient : IUserClient<IUserAccount>
    {
        return provider.CreateUserClient<TClient, IUserAccount>(account, httpService);
    }
}
