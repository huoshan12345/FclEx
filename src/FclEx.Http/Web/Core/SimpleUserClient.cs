using FclEx.Web.Models;
using Microsoft.Extensions.Logging;

namespace FclEx.Web.Core
{
    public abstract class SimpleUserClient : UserClient<UserAccount>
    {
        protected SimpleUserClient(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }
    }

    public abstract class SimpleUserClient<TSession> : UserClient<UserAccount, TSession>
        where TSession : Session, new()
    {
        protected SimpleUserClient(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }
    }
}
