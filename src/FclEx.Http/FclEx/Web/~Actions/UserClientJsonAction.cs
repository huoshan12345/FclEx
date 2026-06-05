namespace FclEx.Web;

public abstract class UserClientJsonAction<TClient, T> : UserClientHttpAction<TClient, T>, IJsonAction<T>
    where TClient : IUserClient
{
    public virtual string? JsonPath { get; } = null;

    protected UserClientJsonAction(TClient client) : base(client) { }

    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultJsonAction.GetResult(this, response);
    public virtual OperationResult<JsonActionContext> CreateContext(HttpResponse response, string json)
        => DefaultJsonAction.CreateContext(this, response, json);
    public virtual OperationResult<T> GetResult(JsonActionContext context)
        => DefaultJsonAction.GetResult(this, context);
    public virtual OperationResult<string> GetJson(HttpResponse response)
        => DefaultJsonAction.GetJson(this, response);
}
