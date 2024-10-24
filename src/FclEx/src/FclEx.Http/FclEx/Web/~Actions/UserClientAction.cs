#if NET6_0_OR_GREATER
namespace FclEx.Web;

public abstract class UserClientAction<TClient, T> : IUserClientAction<TClient, T> where TClient : IUserClient
{
    protected UserClientAction(TClient client)
    {
        Client = client;
        Logger = GetType().Assembly.IsDebug()
            ? client.Logger
            : NullLogger.Instance;
    }

    public virtual ISession Session => Client.Session;
    public IUserAccount Account => Client.Account;
    public TClient Client { get; }
    public ILogger Logger { get; }
    public abstract Task<OperateResult<T>> ExecuteActionAsync(CancellationToken token = default);
}
#endif