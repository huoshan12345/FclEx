namespace FclEx.Http;

/// <summary>
/// Base class for HTTP actions whose response body is JSON.
/// </summary>
/// <typeparam name="T">The result type produced from the selected JSON token.</typeparam>
public abstract class HttpJsonAction<T>: HttpAction<T>, IJsonAction<T>
{
    /// <inheritdoc />
    public virtual string? JsonPath { get; } = null;

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
