using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;
using FclEx.Web.Core;

namespace FclEx.Actions
{
    public abstract class UserClientHttpAction<TClient, T>
        : UserClientAction<TClient, T>, IHttpAction<T>
        where TClient : IUserClient
    {
        protected UserClientHttpAction(TClient client) : base(client)
        {
            HttpService = client.HttpService;
        }

        public IHttpService HttpService { get; }
        public abstract Uri Uri { get; }
        public abstract HttpReqType ReqType { get; }
        public virtual bool IgnoreFailedStatus { get; } = false;
        public virtual bool IgnoreEmptyResponse { get; } = false;

        public abstract OperateResult<T> GetResult(HttpRes response);
        public virtual HttpReq BuildRequest()
        {
            return this.Base<IHttpAction<T>, HttpReq>(m => m.BuildRequest());
        }
        public virtual void ModifyRequest(HttpReq req) { }
        public virtual Task<OperateResult<T>> GetResultAsync(HttpRes response)
        {
            return GetResult(response);
        }
        public override Task<OperateResult<T>> ExecuteAsyncBody(CancellationToken token = default)
        {
            return this.Base<IHttpAction<T>, Task<OperateResult<T>>>(m => m.ExecuteAsyncBody(token));
        }
    }
}
