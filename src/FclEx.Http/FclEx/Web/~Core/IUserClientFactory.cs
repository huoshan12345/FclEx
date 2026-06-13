namespace FclEx.Web;

/// <summary>
/// Creates user-client instances for specific accounts.
/// </summary>
public interface IUserClientFactory<out TClient, in TAccount>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    /// <summary>
    /// Service provider used to resolve supporting services while creating clients.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Creates a client for an account, optionally using a supplied HTTP service.
    /// </summary>
    TClient Create(TAccount account, IHttpService? httpService = null);
}

/// <summary>
/// Creates non-generic user-client instances.
/// </summary>
public interface IUserClientFactory<out TClient> : IUserClientFactory<TClient, IUserAccount>
    where TClient : IUserClient;
