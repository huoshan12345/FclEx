#if NET6_0_OR_GREATER
namespace FclEx.Web;

public abstract class UserClientJsonAction<TClient, T> : UserClientHttpAction<TClient, T>, IJsonAction<T>
    where TClient : IUserClient
{
    public virtual string? JsonResultPath { get; } = null;

    protected UserClientJsonAction(TClient client) : base(client) { }

    public override OperationResult<T> GetResult(HttpResponse response)
        => this.Base<IJsonAction<T>, OperationResult<T>>(m => m.GetResult(response));

    public virtual OperationResult<JsonActionContext> CreateContext(HttpResponse response, string json)
        => this.Base<IJsonAction<T>, OperationResult<JsonActionContext>>(m => m.CreateContext(response, json));

    public virtual OperationResult<T> GetResult(JsonActionContext context)
        => this.Base<IJsonAction<T>, OperationResult<T>>(m => m.GetResult(context));
}
#endif