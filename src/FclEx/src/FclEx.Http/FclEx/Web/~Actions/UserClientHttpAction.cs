namespace FclEx.Web;

public abstract class UserClientHttpAction<TClient, T> : UserClientAction<TClient, T>, IHttpAction<T>
    where TClient : IUserClient
{
    protected UserClientHttpAction(TClient client) : base(client)
    {
        HttpService = client.HttpService;
    }

    public IHttpService HttpService { get; }
    public abstract Uri Uri { get; }
    public abstract HttpMethod Method { get; }

    public virtual bool IsFailed(HttpResponse res) => this.Base<IHttpAction<T>, bool>(m => m.IsFailed(res));
    public virtual OperateResult<T> HandleFailed(HttpResponse res) => this.Base<IHttpAction<T>, OperateResult<T>>(m => m.HandleFailed(res));
    public abstract OperateResult<T> GetResult(HttpResponse response);
    public virtual HttpRequest BuildRequest() => this.Base<IHttpAction<T>, HttpRequest>(m => m.BuildRequest());
    public virtual void ModifyRequest(HttpRequest req) { }
    public virtual Task<OperateResult<T>> GetResultAsync(HttpResponse response) => GetResult(response);
    public override Task<OperateResult<T>> ExecuteAsyncBody(CancellationToken token = default)
        => this.Base<IHttpAction<T>, Task<OperateResult<T>>>(m => m.ExecuteAsyncBody(token));
}