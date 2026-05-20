namespace FclEx.Http;

public abstract class JsonAction<T> : HttpResponseHandler<T>, IJsonAction<T>
{
    public virtual string? JsonResultPath => null;
    public virtual OperationResult<string> GetJson(HttpResponse response)
        => DefaultJsonAction.GetJson(this, response);
    public virtual OperationResult<JsonActionContext> CreateContext(HttpResponse response, string json)
        => DefaultJsonAction.CreateContext(this, response, json);
    public virtual OperationResult<T> GetResult(JsonActionContext context)
        => DefaultJsonAction.GetResult(this, context);
    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultJsonAction.GetResult(this, response);
}

public abstract class JsonAction : JsonAction<Unit>, IJsonAction
{
    public override OperationResult GetResult(JsonActionContext context) => Operation.Success();
}