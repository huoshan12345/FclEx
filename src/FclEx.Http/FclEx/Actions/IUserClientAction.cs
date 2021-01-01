using FclEx.Web.Core;

namespace FclEx.Actions
{
    public interface IUserClientAction<out TClient, T> : IAbstractAction<T> where TClient : IUserClient
    {
        public TClient Client { get; }
        public ISession Session => Client.Session;
        public IUserAccount Account => Client.Account;
    }
}
