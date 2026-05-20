namespace FclEx.Http;

public abstract class JsonpAction<T> : HttpAction<T>, IJsonpAction<T>
{
    public virtual string? JsonResultPath => null;
    public override HttpMethod Method => HttpMethod.Get;
    public abstract string CallbackParamName { get; }
    public override void ModifyRequest(HttpRequest request)
        => DefaultJsonpAction.ModifyRequest(this, request);
    public virtual OperationResult<string> GetJson(HttpResponse response) 
        => DefaultJsonpAction.GetJson(this, response);
    public virtual OperationResult<JsonActionContext> CreateContext(HttpResponse response, string json)
        => DefaultJsonAction.CreateContext(this, response, json);
    public virtual OperationResult<T> GetResult(JsonActionContext context)
        => DefaultJsonAction.GetResult(this, context);
    public override OperationResult<T> GetResult(HttpResponse response) 
        => DefaultJsonAction.GetResult(this, response);
}