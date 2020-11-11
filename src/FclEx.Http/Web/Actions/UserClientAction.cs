using System.Diagnostics.CodeAnalysis;
using FclEx.Actions;
using FclEx.Web.Core;
using FclEx.Web.Models;

namespace FclEx.Web.Actions
{
    public abstract class UserClientAction<TClient, TAccount, TSession, T> : AbstractHttpAction<T>
        where TClient : IUserClient, IHasAccount<TAccount>, IHasSession<TSession>
        where TSession : ISession
    {
        protected TClient Client { get; }
        protected TSession Session => Client.Session;
        [MaybeNull] protected TAccount Account => Client.Account;

        protected UserClientAction(TClient client)
            : base(client.HttpService)
        {
            Client = client;
            // Logger = client.Logger;
        }
    }

    public abstract class UserClientAction<TClient, TSession, T> : UserClientAction<TClient, UserAccount, TSession, T>
        where TClient : IUserClient, IHasAccount<UserAccount>, IHasSession<TSession>
        where TSession : ISession
    {
        protected UserClientAction(TClient client) : base(client)
        {
        }
    }
}
