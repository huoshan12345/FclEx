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