namespace FclEx.Http;

public abstract class HttpJsonAction<T>: HttpAction<T>, IJsonAction<T>
{
    public virtual string? JsonResultPath { get; } = null;
    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultJsonAction.GetResult(this, response);
    public virtual OperationResult<JsonActionContext> CreateContext(HttpResponse response, string json)
        => DefaultJsonAction.CreateContext(this, response, json);
    public virtual OperationResult<T> GetResult(JsonActionContext context)
        => DefaultJsonAction.GetResult(this, context);
    public virtual OperationResult<string> GetJson(HttpResponse response)
        => DefaultJsonAction.GetJson(this, response);
}
