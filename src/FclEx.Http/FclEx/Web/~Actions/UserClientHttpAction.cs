namespace FclEx.Web;

/// <summary>
/// Base class for user-client actions that build and send an HTTP request.
/// </summary>
public abstract class UserClientHttpAction<TClient, TAccount, T>(TClient client)
    : UserClientAction<TClient, TAccount, T>(client), IHttpAction<T>
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    /// <inheritdoc />
    public abstract Uri Uri { get; }

    /// <inheritdoc />
    public abstract HttpMethod Method { get; }

    /// <inheritdoc />
    public virtual bool EnsureSuccessStatusCode { get; } = true;

    /// <inheritdoc />
    public virtual IHttpService HttpService { get; } = client.HttpService;

    /// <inheritdoc />
    public abstract OperationResult<T> GetResult(HttpResponse response);

    /// <inheritdoc />
    public virtual Task<HttpResponse> GetResponseAsync(HttpRequest request, CancellationToken token = default)
        => DefaultHttpAction.GetResponseAsync(this, request, token);

    /// <inheritdoc />
    public virtual Task<OperationResult<HttpResponse>> HandleResponseAsync(HttpResponse response)
        => DefaultHttpAction.HandleResponseAsync(this, response);

    /// <inheritdoc />
    public virtual HttpRequest BuildRequest() => DefaultHttpAction.BuildRequest(this);

    /// <inheritdoc />
    public virtual void ModifyRequest(HttpRequest request) { }

    /// <inheritdoc />
    public virtual Task<OperationResult<T>> GetResultAsync(HttpResponse response)
        => DefaultHttpResponseHandler.GetResultAsync(this, response);

    /// <inheritdoc />
    public override Task<OperationResult<T>> ExecuteCoreAsync(CancellationToken token = default)
        => DefaultHttpAction.ExecuteCoreAsync(this, token);
}

/// <summary>
/// Base class for HTTP actions that operate through a non-generic user client.
/// </summary>
public abstract class UserClientHttpAction<TClient, T>(TClient client)
    : UserClientHttpAction<TClient, IUserAccount, T>(client)
    where TClient : IUserClient;
