namespace FclEx.Http;

/// <summary>
/// Base class for handling a JSON response without sending the request itself.
/// </summary>
/// <typeparam name="T">The result type produced from the selected JSON token.</typeparam>
public abstract class JsonAction<T> : HttpResponseHandler<T>, IJsonAction<T>
{
    /// <inheritdoc />
    public virtual string? JsonResultPath => null;

    /// <inheritdoc />
    public virtual OperationResult<string> GetJson(HttpResponse response)
        => DefaultJsonAction.GetJson(this, response);

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

/// <summary>
/// Base class for JSON response handlers that only need success or failure.
/// </summary>
public abstract class JsonAction : JsonAction<Unit>, IJsonAction
{
    /// <inheritdoc />
    public override OperationResult GetResult(JsonActionContext context) => Operation.Success();
}
