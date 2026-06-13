namespace FclEx.Web;

/// <summary>
/// Creates user-client instances from a service provider and assigns account/service state.
/// </summary>
/// <typeparam name="TClient">The concrete user-client type.</typeparam>
/// <typeparam name="TAccount">The account type accepted by the client.</typeparam>
public class UserClientFactory<TClient, TAccount> : IUserClientFactory<TClient, TAccount>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    /// <summary>
    /// Initializes a factory backed by a service provider.
    /// </summary>
    /// <param name="serviceProvider">The provider used to resolve <typeparamref name="TClient"/>.</param>
    public UserClientFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// The provider used to resolve clients.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Resolves a client, assigns its account, and optionally replaces its HTTP service.
    /// </summary>
    /// <param name="account">The account assigned to the resolved client.</param>
    /// <param name="httpService">Optional HTTP service assigned to the client.</param>
    /// <returns>The resolved and initialized client.</returns>
    public virtual TClient Create(TAccount account, IHttpService? httpService = null)
    {
        var client = ServiceProvider.GetRequiredService<TClient>();
        client.Account = account;
        if (httpService is not null)
            client.HttpService = httpService;
        return client;
    }
}

/// <summary>
/// Creates user clients that use <see cref="IUserAccount"/>.
/// </summary>
/// <typeparam name="TClient">The concrete user-client type.</typeparam>
public class UserClientFactory<TClient>(IServiceProvider serviceProvider)
    : UserClientFactory<TClient, IUserAccount>(serviceProvider), IUserClientFactory<TClient>
    where TClient : IUserClient;
