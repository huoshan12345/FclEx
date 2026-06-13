namespace FclEx.Web;

/// <summary>
/// Base class for user-client HTTP actions whose response body is JSON.
/// </summary>
public abstract class UserClientJsonAction<TClient, T> : UserClientHttpAction<TClient, T>, IJsonAction<T>
    where TClient : IUserClient
{
    /// <inheritdoc />
    public virtual string? JsonPath { get; } = null;

    /// <summary>
    /// Creates a JSON action bound to a user client.
    /// </summary>
    protected UserClientJsonAction(TClient client) : base(client) { }

    /// <inheritdoc />
    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultJsonAction.GetResult(this, response);

    /// <inheritdoc />
    public virtual OperationResult<JsonActionContext> CreateContext(HttpResponse response, string json)
        => DefaultJsonAction.CreateContext(this, response, json);

    /// <inheritdoc />
    public virtual OperationResult<T> GetResult(JsonActionContext context)
        => DefaultJsonAction.GetResult(this, context);

    /// <inheritdoc />
    public virtual OperationResult<string> GetJson(HttpResponse response)
        => DefaultJsonAction.GetJson(this, response);
}
