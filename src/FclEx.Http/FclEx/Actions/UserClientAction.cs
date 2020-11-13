using FclEx.Web.Core;
using FclEx.Web.Models;

namespace FclEx.Actions
{
    public abstract class UserClientAction<TClient, T> : AbstractHttpAction<T> where TClient : IUserClient
    {
        protected TClient Client { get; }
        protected ISession Session => Client.Session;
        protected IUserAccount? Account => Client.Account;

        protected UserClientAction(TClient client)
            : base(client.HttpService)
        {
            Client = client;
            // Logger = client.Logger;
        }
    }
}
