namespace FclEx.Web;

public interface IUserClientFactory<out TClient, in TAccount>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    IServiceProvider ServiceProvider { get; }
    TClient Create(TAccount account, IHttpService? httpService = null);
}

public interface IUserClientFactory<out TClient> : IUserClientFactory<TClient, IUserAccount>
    where TClient : IUserClient;