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

public static class DefaultJsonAction
{
    public static OperationResult<T> GetResult<T>(IJsonAction<T> action, HttpResponse response)
    {
        return action.GetJson(response)
            .Then(m => action.CreateContext(response, m))
            .Then(action.GetResult);
    }

    public static OperationResult<string> GetJson<T>(IJsonAction<T> action, HttpResponse response)
    {
        var str = response.ResponseString;
        return str.IsPossibleJson()
            ? Operation.Success(response.ResponseString)
            : Operation.Error<string>("The response string is not a valid json: " + str.Truncate(256));
    }

    public static OperationResult<JsonActionContext> CreateContext<T>(IJsonAction<T> action, HttpResponse response, string json)
    {
        var context = new JsonActionContext(response, json, action.JsonResultPath);
        if (context.ResultTokens.IsNotEmpty())
            return context;

        const string msg = "The result object does not exist in json";
        var error = action.JsonResultPath == null ? msg : msg + " at " + action.JsonResultPath;
        error = error + ": " + context.Json.Truncate(256);
        return error;
    }

    public static OperationResult<T> GetResult<T>(IJsonAction<T> action, JsonActionContext context)
    {
        return context.ResultToken is { } token
            ? token.ToObject<T>()!
            : nameof(context.ResultToken) + " is null";
    }
}

public abstract class JsonAction<T> : HttpResponseHandler<T>, IJsonAction<T>
{
    public virtual string? JsonResultPath => null;
    public virtual OperationResult<string> GetJson(HttpResponse response) => DefaultJsonAction.GetJson(this, response);
    public virtual OperationResult<JsonActionContext> CreateContext(HttpResponse response, string json) 
        => DefaultJsonAction.CreateContext(this, response, json);
    public virtual OperationResult<T> GetResult(JsonActionContext context) => DefaultJsonAction.GetResult(this, context);
}

public abstract class JsonAction : JsonAction<Unit>, IJsonAction
{
    public override OperationResult GetResult(JsonActionContext context) => Operation.Success();
}