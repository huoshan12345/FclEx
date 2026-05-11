namespace FclEx.Http;

public interface IJsonpAction<T> : IJsonAction<T>, IHttpAction<T>
{
    string CallbackParamName { get; }

#if NET6_0_OR_GREATER
    string? IJsonAction<T>.JsonResultPath => null;
    HttpMethod IHttpAction<T>.Method => HttpMethod.Get;
    void IHttpAction<T>.ModifyRequest(HttpRequest request) 
        => DefaultJsonpAction.ModifyRequest(this, request);
    OperationResult<string> IJsonAction<T>.GetJson(HttpResponse response)
        => DefaultJsonpAction.GetJson(this, response);
#endif
}

public static class DefaultJsonpAction
{
    public static void ModifyRequest<T>(IJsonpAction<T> action, HttpRequest request)
    {
        request.AddQueryParam(action.CallbackParamName, Regexes.CallBackName);
    }

    public static OperationResult<string> GetJson<T>(IJsonpAction<T> action, HttpResponse response)
    {
        var match = Regexes.CallBackContent.Match(response.ResponseString);
        return match.Success
            ? Operation.Success(match.Value)
            : Operation.Error<string>("Failed to parse callback: " + response.ResponseString.Truncate(200));
    }
}

public abstract class JsonpAction<T> : HttpAction<T>, IJsonpAction<T>
{
    public virtual string? JsonResultPath => null;
    public abstract string CallbackParamName { get; }
    public virtual OperationResult<string> GetJson(HttpResponse response) => DefaultJsonpAction.GetJson(this, response);
    public virtual OperationResult<JsonActionContext> CreateContext(HttpResponse response, string json)
        => DefaultJsonAction.CreateContext(this, response, json);
    public abstract OperationResult<T> GetResult(JsonActionContext context);
    public override void ModifyRequest(HttpRequest request)
        => DefaultJsonpAction.ModifyRequest(this, request);
}
