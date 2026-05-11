namespace FclEx.Http;

public interface IJsonpAction<T> : IJsonAction<T>, IHttpAction<T>
{
    string CallbackParamName { get; }

#if NET6_0_OR_GREATER
    string? IJsonAction<T>.JsonResultPath => null;
    HttpMethod IHttpAction<T>.Method => HttpMethod.Get;

    void IHttpAction<T>.ModifyRequest(HttpRequest request)
    {
        request.AddQueryParam(CallbackParamName, Regexes.CallBackName);
    }

    OperationResult<string> IJsonAction<T>.GetJson(HttpResponse response)
    {
        var match = Regexes.CallBackContent.Match(response.ResponseString);
        return match.Success
            ? Operation.Success(match.Value)
            : Operation.Error<string>("Failed to parse callback: " + response.ResponseString.Truncate(200));
    }
#endif
}