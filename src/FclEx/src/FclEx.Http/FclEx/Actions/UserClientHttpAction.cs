using System.Threading;
using FclEx.Web;

namespace FclEx.Actions;

public abstract class UserClientHttpAction<TClient, T> : UserClientAction<TClient, T>, IHttpAction<T>
    where TClient : IUserClient
{
    protected UserClientHttpAction(TClient client) : base(client)
    {
        HttpService = client.HttpService;
    }

    public IHttpService HttpService { get; }
    public abstract Uri Uri { get; }
    public abstract HttpReqType ReqType { get; }

    public virtual bool IsFailed(HttpRes res) => this.Base<IHttpAction<T>, bool>(m => m.IsFailed(res));
    public virtual OperateResult<T> HandleFailed(HttpRes res) => this.Base<IHttpAction<T>, OperateResult<T>>(m => m.HandleFailed(res));
    public abstract OperateResult<T> GetResult(HttpRes response);
    public virtual HttpReq BuildRequest() => this.Base<IHttpAction<T>, HttpReq>(m => m.BuildRequest());
    public virtual void ModifyRequest(HttpReq req) { }
    public virtual Task<OperateResult<T>> GetResultAsync(HttpRes response) => GetResult(response);
    public override Task<OperateResult<T>> ExecuteAsyncBody(CancellationToken token = default)
        => this.Base<IHttpAction<T>, Task<OperateResult<T>>>(m => m.ExecuteAsyncBody(token));
}