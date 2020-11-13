using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;
using FclEx.Web.Core;
using Microsoft.Extensions.Logging;

namespace FclEx.Web
{
    public class TestUserClient : UserClient
    {
        public TestUserClient(ILoggerFactory loggerFactory) : base(loggerFactory: loggerFactory)
        {
        }

        protected override Task<OperateResult> LoginInternal(CancellationToken token)
        {
            return OperateResult.Success.ToTask();
        }
    }
}
