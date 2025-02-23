#if NET6_0_OR_GREATER
namespace FclEx.Http;

public interface IUserClientAction<out TClient, T> : IAbstractAction<T> where TClient : IUserClient
{
    TClient Client { get; }
    IUserClientSession Session => Client.Session;
    IUserAccount Account => Client.Account;

}
#endif