namespace FclEx.Http;

public interface IUserClientAction<out TClient, out TAccount, T> : IAbstractAction<T>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    TClient Client { get; }
    IUserClientSession Session
#if NET6_0_OR_GREATER
        => Client.Session;
#else
     { get; }
#endif

    TAccount Account
#if NET6_0_OR_GREATER
        => Client.Account;
#else
     { get; }
#endif
}

public interface IUserClientAction<out TClient, T> : IUserClientAction<TClient, IUserAccount, T>
    where TClient : IUserClient;