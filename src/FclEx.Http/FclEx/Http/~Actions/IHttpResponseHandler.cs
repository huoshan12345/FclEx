namespace FclEx.Http;

public interface IHttpResponseHandler<T>
{
    OperationResult<T> GetResult(HttpResponse response);
}