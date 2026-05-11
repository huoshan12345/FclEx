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
    {
        return GetJson(response)
            .Then(m => CreateContext(response, m))
            .Then(GetResult);
    }

    OperationResult<string> GetJson(HttpResponse response)
    {
        var str = response.ResponseString;
        return str.IsPossibleJson()
            ? Operation.Success(response.ResponseString)
            : Operation.Error<string>("The response string is not a valid json: " + str.Truncate(256));
    }

    OperationResult<JsonActionContext> CreateContext(HttpResponse response, string json)
    {
        var context = new JsonActionContext(response, json, JsonResultPath);
        if (context.ResultTokens.IsNotEmpty())
            return context;

        const string msg = "The result object does not exist in json";
        var error = JsonResultPath == null ? msg : msg + " at " + JsonResultPath;
        error = error + ": " + context.Json.Truncate(256);
        return error;
    }

    OperationResult<T> GetResult(JsonActionContext context)
    {
        return context.ResultToken is { } token
            ? token.ToObject<T>()!
            : nameof(context.ResultToken) + " is null";
    }
#endif
}

public interface IJsonAction : IJsonAction<Unit>
{
#if NET6_0_OR_GREATER
    OperationResult IJsonAction<Unit>.GetResult(JsonActionContext context) => Operation.Success();
#endif
}
