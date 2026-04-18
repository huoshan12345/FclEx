#if NET6_0_OR_GREATER
namespace FclEx.Http;

public interface IJsonpAction<T> : IJsonAction<T>, IHttpAction<T>
{
    string? IJsonAction<T>.JsonResultPath => null;
    HttpMethod IHttpAction<T>.Method => HttpMethod.Get;
    string CallbackParamName { get; }

    void IHttpAction<T>.ModifyRequest(HttpRequest request)
    {
        request.AddQueryParam(CallbackParamName, CommonWebRegexes.CallBackName);
    }

    OperationResult<string> IJsonAction<T>.GetJson(HttpResponse response)
    {
        var match = CommonWebRegexes.CallBackContent.Match(response.ResponseString);
        return match.Success
            ? Operation.Success(match.Value)
            : Operation.Error<string>("Failed to parse callback: " + response.ResponseString.Truncate(200));
    }
}
#endif