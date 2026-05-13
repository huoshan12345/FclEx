namespace FclEx.Web;

public abstract class UserClientAction<TClient, TAccount, T> :
#if !NET6_0_OR_GREATER
    PipelineAction<T>,
#endif
    IUserClientAction<TClient, TAccount, T>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    protected UserClientAction(TClient client)
    {
        Client = client;
        Logger = client.Logger;
    }

    public virtual IUserClientState State => Client.State;
    public virtual IUserClientSession Session => Client.Session;
    public virtual TAccount Account => Client.Account;
    public TClient Client { get; }
    public ILogger Logger { get; }
#if NET6_0_OR_GREATER
    public abstract Task<OperationResult<T>> ExecuteActionAsync(CancellationToken token = default);
#endif
}

public abstract class UserClientAction<TClient, T> : UserClientAction<TClient, IUserAccount, T>, IUserClientAction<TClient, T>
    where TClient : IUserClient
{
    protected UserClientAction(TClient client) : base(client)
    {
    }
}
