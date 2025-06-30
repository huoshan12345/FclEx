#if NET6_0_OR_GREATER
namespace FclEx.Http;

public interface IUserClientAction<out TClient, out TAccount, T> : IAbstractAction<T>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    TClient Client { get; }
    IUserClientSession Session => Client.Session;
    TAccount Account => Client.Account;
}

public interface IUserClientAction<out TClient, T> : IUserClientAction<TClient, IUserAccount, T>
    where TClient : IUserClient;
#endif