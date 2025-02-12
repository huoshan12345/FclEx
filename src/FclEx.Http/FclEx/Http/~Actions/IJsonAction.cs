#if NET6_0_OR_GREATER
namespace FclEx.Http;

public interface IJsonAction<T> : IHttpResponseHandler<T>
{
    string? JsonResultPath { get; }

    OperationResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse response)
    {
        var (successful, str, ex, _) = GetJson(response);
        if (!successful)
            return ex!;

        var context = new JsonActionContext(response, str!, JsonResultPath);

        return IsFailed(context) 
            ? HandleFailed(context) 
            : GetResult(context);
    }

    OperationResult<string> GetJson(HttpResponse response)
    {
        var str = response.ResponseString;
        return str.IsPossibleJson()
            ? Operation.Success(response.ResponseString)
            : Operation.Error<string>("The response string is not a valid json: " + str.Truncate(256));
    }

    bool IsFailed(JsonActionContext context) => !context.ResultTokens.Any();

    OperationResult<T> HandleFailed(JsonActionContext context)
    {
        const string msg = "The result object does not exist in json";
        var error = JsonResultPath == null ? msg : msg + " at " + JsonResultPath;
        error = error + ": " + context.Json.Truncate(256);
        return error;
    }

    OperationResult<T> GetResult(JsonActionContext context)
    {
        return context.ResultToken is { } token
            ? token.Deserialize<T>()!
            : nameof(context.ResultToken) + " is null";
    }
}

public interface IJsonAction : IJsonAction<Unit>
{
    OperationResult IJsonAction<Unit>.GetResult(JsonActionContext context) => Operation.Success();
}
#endif