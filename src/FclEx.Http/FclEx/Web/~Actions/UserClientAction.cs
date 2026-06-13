namespace FclEx.Web;

/// <summary>
/// Base class for pipeline actions that operate through a user client.
/// </summary>
public abstract class UserClientAction<TClient, TAccount, T> :
#if !NET6_0_OR_GREATER
    PipelineAction<T>,
#endif
    IUserClientAction<TClient, TAccount, T>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    /// <summary>
    /// Creates an action bound to a user client and reuses the client's logger.
    /// </summary>
    protected UserClientAction(TClient client)
    {
        Client = client;
        Logger = client.Logger;
    }

    /// <inheritdoc />
    public virtual IUserClientState State => Client.State;

    /// <summary>
    /// Session information exposed by the owning client.
    /// </summary>
    public virtual IUserClientSession Session => Client.Session;

    /// <inheritdoc />
    public virtual TAccount Account => Client.Account;

    /// <inheritdoc />
    public TClient Client { get; }

    /// <summary>
    /// Logger associated with the owning client.
    /// </summary>
    public ILogger Logger { get; }
#if NET6_0_OR_GREATER
    /// <inheritdoc />
    public abstract Task<OperationResult<T>> ExecuteCoreAsync(CancellationToken token = default);
#endif
}

/// <summary>
/// Base class for pipeline actions that operate through a non-generic user client.
/// </summary>
public abstract class UserClientAction<TClient, T> : UserClientAction<TClient, IUserAccount, T>, IUserClientAction<TClient, T>
    where TClient : IUserClient
{
    protected UserClientAction(TClient client) : base(client)
    {
    }
}
