#if NET6_0_OR_GREATER
namespace FclEx.Web;

public abstract class UserClientHttpAction<TClient, T>(TClient client) : UserClientAction<TClient, T>(client), IHttpAction<T>
    where TClient : IUserClient
{
    public abstract Uri Uri { get; }
    public abstract HttpMethod Method { get; }

    public virtual IHttpService HttpService { get; } = client.HttpService;
    public virtual bool IsFailed(HttpResponse res) => this.Base<IHttpAction<T>, bool>(m => m.IsFailed(res));
    public virtual OperateResult<T> HandleFailed(HttpResponse res) => this.Base<IHttpAction<T>, OperateResult<T>>(m => m.HandleFailed(res));
    public abstract OperateResult<T> GetResult(HttpResponse response);
    public virtual HttpRequest BuildRequest() => this.Base<IHttpAction<T>, HttpRequest>(m => m.BuildRequest());
    public virtual void ModifyRequest(HttpRequest req) { }
    public virtual Task<OperateResult<T>> GetResultAsync(HttpResponse response) => GetResult(response);
    public override Task<OperateResult<T>> ExecuteActionAsync(CancellationToken token = default)
        => this.Base<IHttpAction<T>, Task<OperateResult<T>>>(m => m.ExecuteActionAsync(token));
}
#endif