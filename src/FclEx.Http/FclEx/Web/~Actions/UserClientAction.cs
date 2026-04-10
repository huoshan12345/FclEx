#if NET6_0_OR_GREATER
namespace FclEx.Web;

public abstract class UserClientAction<TClient, TAccount, T> : IUserClientAction<TClient, TAccount, T>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    protected UserClientAction(TClient client)
    {
        Client = client;
        Logger = client.Logger;
    }

    public virtual IUserClientSession Session => Client.Session;
    public virtual IUserAccount Account => Client.Account;
    public TClient Client { get; }
    public ILogger Logger { get; }
    public abstract Task<OperationResult<T>> ExecuteActionAsync(CancellationToken token = default);
}

public abstract class UserClientAction<TClient, T> : UserClientAction<TClient, IUserAccount, T>, IUserClientAction<TClient, T>
    where TClient : IUserClient
{
    protected UserClientAction(TClient client) : base(client)
    {
    }
}
#endif