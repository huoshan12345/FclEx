using System.Threading;
using FclEx.Web;
using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Actions;

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
    public abstract Task<OperateResult<T>> ExecuteAsyncBody(CancellationToken token = default);
}