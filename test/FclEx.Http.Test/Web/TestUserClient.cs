using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;
using FclEx.Web.Core;
using FclEx.Web.Models;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Test.Web
{
    public class TestUserClient : UserClient<UserAccount, Session>
    {
        public TestUserClient(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        protected override Task<OperateResult> LoginInternal(CancellationToken token)
        {
            return OperateResult.Success.ToTask();
        }
    }
}
