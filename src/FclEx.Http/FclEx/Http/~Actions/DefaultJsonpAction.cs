namespace FclEx.Http;

public static class DefaultJsonpAction
{
    public static void ModifyRequest<T>(IJsonpAction<T> action, HttpRequest request)
    {
        request.AddQueryParam(action.CallbackParamName, Regexes.CallbackName);
    }

    public static OperationResult<string> GetJson<T>(IJsonpAction<T> action, HttpResponse response)
    {
        var match = Regexes.CallbackContent.Match(response.ResponseString);
        return match.Success
            ? Operation.Success(match.Value)
            : Operation.Error<string>("Failed to parse callback: " + response.ResponseString.Truncate(200));
    }
}