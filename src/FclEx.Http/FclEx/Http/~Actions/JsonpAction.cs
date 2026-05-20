namespace FclEx.Http;

/// <summary>
/// Base class for HTTP JSONP actions.
/// </summary>
/// <typeparam name="T">The result type produced from the JSONP payload.</typeparam>
public abstract class JsonpAction<T> : HttpAction<T>, IJsonpAction<T>
{
    /// <inheritdoc />
    public virtual string? JsonResultPath => null;

    /// <inheritdoc />
    public override HttpMethod Method => HttpMethod.Get;

    /// <inheritdoc />
    public abstract string CallbackParamName { get; }

    /// <inheritdoc />
    public override void ModifyRequest(HttpRequest request)
        => DefaultJsonpAction.ModifyRequest(this, request);

    /// <inheritdoc />
    public virtual OperationResult<string> GetJson(HttpResponse response) 
        => DefaultJsonpAction.GetJson(this, response);

    /// <inheritdoc />
    public virtual OperationResult<JsonActionContext> CreateContext(HttpResponse response, string json)
        => DefaultJsonAction.CreateContext(this, response, json);

    /// <inheritdoc />
    public virtual OperationResult<T> GetResult(JsonActionContext context)
        => DefaultJsonAction.GetResult(this, context);

    /// <inheritdoc />
    public override OperationResult<T> GetResult(HttpResponse response) 
        => DefaultJsonAction.GetResult(this, response);
}
