namespace FclEx.Web;

public class UserClientFactory<TClient, TAccount> : IUserClientFactory<TClient, TAccount>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    public UserClientFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    public virtual TClient Create(TAccount account, IHttpService? httpService = null)
    {
        var client = ServiceProvider.GetRequiredService<TClient>();
        client.Account = account;
        if (httpService is not null)
            client.HttpService = httpService;
        return client;
    }
}

public class UserClientFactory<TClient>(IServiceProvider serviceProvider)
    : UserClientFactory<TClient, IUserAccount>(serviceProvider)
    where TClient : IUserClient;
