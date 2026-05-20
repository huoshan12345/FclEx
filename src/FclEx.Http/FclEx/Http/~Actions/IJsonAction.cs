namespace FclEx.Http;

public interface IJsonAction<T> : IHttpResponseHandler<T>
{
    string? JsonResultPath
#if NET6_0_OR_GREATER
        => null;
#else
    { get; }
#endif

#if NET6_0_OR_GREATER
    OperationResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse response)
        => DefaultJsonAction.GetResult(this, response);
#endif

    OperationResult<string> GetJson(HttpResponse response)
#if NET6_0_OR_GREATER
        => DefaultJsonAction.GetJson(this, response);
#else
    ;
#endif

    OperationResult<JsonActionContext> CreateContext(HttpResponse response, string json)
#if NET6_0_OR_GREATER
        => DefaultJsonAction.CreateContext(this, response, json);
#else
    ;
#endif

    OperationResult<T> GetResult(JsonActionContext context)
#if NET6_0_OR_GREATER
        => DefaultJsonAction.GetResult(this, context);
#else
    ;
#endif
}

public interface IJsonAction : IJsonAction<Unit>
{
#if NET6_0_OR_GREATER
    OperationResult IJsonAction<Unit>.GetResult(JsonActionContext context) => Operation.Success();
#endif
}