using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Actions;
using FclEx.Http.Core;
using FclEx.Utils;
using FclEx.Web.Core;

namespace FclEx.Web
{
    public class TestAction : UserClientAction<TestUserClient, string>
    {
        public TestAction(TestUserClient client) : base(client)
        {
        }

        protected override Task<OperateResult<string>> HandleResponseAsync(HttpRes response)
        {
            throw new NotImplementedException();
        }

        protected override string Url { get; }
        protected override HttpReqType ReqType { get; }
    }
}
