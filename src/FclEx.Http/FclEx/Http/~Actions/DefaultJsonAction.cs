namespace FclEx.Http;

public static class DefaultJsonAction
{
    public static OperationResult<T> GetResult<T>(IJsonAction<T> action, HttpResponse response)
    {
        return action.GetJson(response)
            .Then(m => action.CreateContext(response, m))
            .Then(m =>
            {
                using var _ = m;
                return action.GetResult(m);
            });
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