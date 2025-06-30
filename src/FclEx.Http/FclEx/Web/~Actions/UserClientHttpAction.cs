#if NET6_0_OR_GREATER
namespace FclEx.Web;

public abstract class UserClientHttpAction<TClient, TAccount, T>(TClient client) : UserClientAction<TClient, TAccount, T>(client), IHttpAction<T>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    public abstract Uri Uri { get; }
    public abstract HttpMethod Method { get; }

    public virtual IHttpService HttpService { get; } = client.HttpService;
    public virtual bool IsFailed(HttpResponse response) => this.Base<IHttpAction<T>, bool>(m => m.IsFailed(response));
    public virtual OperationResult<T> HandleFailed(HttpResponse response) => this.Base<IHttpAction<T>, OperationResult<T>>(m => m.HandleFailed(response));
    public abstract OperationResult<T> GetResult(HttpResponse response);
    public virtual HttpRequest BuildRequest() => this.Base<IHttpAction<T>, HttpRequest>(m => m.BuildRequest());
    public virtual void ModifyRequest(HttpRequest request) { }
    public virtual Task<OperationResult<T>> GetResultAsync(HttpResponse response) => GetResult(response);
    public override Task<OperationResult<T>> ExecuteActionAsync(CancellationToken token = default)
        => this.Base<IHttpAction<T>, Task<OperationResult<T>>>(m => m.ExecuteActionAsync(token));
}

public abstract class UserClientHttpAction<TClient, T>(TClient client)
    : UserClientHttpAction<TClient, IUserAccount, T>(client)
    where TClient : IUserClient;
#endif