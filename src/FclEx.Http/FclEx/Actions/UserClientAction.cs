using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;
using FclEx.Web.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Actions
{
    public abstract class UserClientAction<TClient, T> : IUserClientAction<TClient, T> where TClient : IUserClient
    {
        protected UserClientAction(TClient client)
        {
            Client = client;
            HttpService = client.HttpService;
        }

        public ISession Session => Client.Session;
        public IUserAccount Account => Client.Account ?? throw new ArgumentNullException(nameof(Account));
        public abstract HttpReqType ReqType { get; }
        public TClient Client { get; }
        public IHttpService HttpService { get; set; }
        public virtual bool LoginAndRetry { get; } = false;
        public abstract OperateResult<T> GetResult(HttpRes response);
        public virtual bool HandleResponseOnError { get; } = false;
        public ILogger Logger { get; set; } = NullLogger.Instance;
        public virtual void ModifyRequest(HttpReq req) { }
        public abstract Uri Uri { get; }
    }
}
