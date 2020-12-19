using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;
using FclEx.Web.Core;
using FclEx.Web.Models;

namespace FclEx.Actions
{
    public abstract class UserClientAction<TClient, T> : AbstractHttpAction<T> where TClient : IUserClient
    {
        protected virtual TClient Client { get; }
        protected virtual ISession Session => Client.Session;
        protected virtual IUserAccount? Account => Client.Account;
        protected virtual bool LoginAndRetry { get; } = false;

        protected UserClientAction(TClient client)
            : base(client.HttpService)
        {
            Client = client;
            // Logger = client.Logger;
        }

        protected override async Task<OperateResult<T>> ExecuteInternalAsync(CancellationToken token = default)
        {
            var result = await base.ExecuteInternalAsync(token).DonotCapture();
            if (!result.Successful && !Client.IsOnline && LoginAndRetry)
            {
                await Client.FakeLogin(true, token).DonotCapture();
                result = await base.ExecuteInternalAsync(token).DonotCapture();
            }
            return result;
        }
    }
}
