namespace FclEx.Web;

public abstract class UserClientHttpAction<TClient, TAccount, T>(TClient client)
    : UserClientAction<TClient, TAccount, T>(client), IHttpAction<T>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    public abstract Uri Uri { get; }
    public abstract HttpMethod Method { get; }
    public bool EnsureSuccessStatusCode { get; } = true;
    public virtual IHttpService HttpService { get; } = client.HttpService;

    public abstract OperationResult<T> GetResult(HttpResponse response);
    public virtual Task<OperationResult<HttpResponse>> HandleResponseAsync(HttpResponse response)
        => DefaultHttpAction.HandleResponseAsync(this, response);
    public virtual HttpRequest BuildRequest() => DefaultHttpAction.BuildRequest(this);
    public virtual void ModifyRequest(HttpRequest request) { }
    public virtual Task<OperationResult<T>> GetResultAsync(HttpResponse response)
        => DefaultHttpResponseHandler.GetResultAsync(this, response);
    public override Task<OperationResult<T>> ExecuteActionAsync(CancellationToken token = default)
        => DefaultHttpAction.ExecuteActionAsync(this, token);
}

public abstract class UserClientHttpAction<TClient, T>(TClient client)
    : UserClientHttpAction<TClient, IUserAccount, T>(client)
    where TClient : IUserClient;