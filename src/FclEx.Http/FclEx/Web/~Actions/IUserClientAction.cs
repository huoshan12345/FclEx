namespace FclEx.Web;

/// <summary>
/// Pipeline action executed in the context of a user client and its account.
/// </summary>
public interface IUserClientAction<out TClient, out TAccount, T> : IPipelineAction<T>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    /// <summary>
    /// User client that owns the action.
    /// </summary>
    TClient Client { get; }

    /// <summary>
    /// Current mutable state of the owning client.
    /// </summary>
    IUserClientState State
#if NET6_0_OR_GREATER
        => Client.State;
#else
     { get; }
#endif

    /// <summary>
    /// Account used by the owning client.
    /// </summary>
    TAccount Account
#if NET6_0_OR_GREATER
        => Client.Account;
#else
     { get; }
#endif
}

/// <summary>
/// Pipeline action executed in the context of a non-generic user client.
/// </summary>
public interface IUserClientAction<out TClient, T> : IUserClientAction<TClient, IUserAccount, T>
    where TClient : IUserClient;
