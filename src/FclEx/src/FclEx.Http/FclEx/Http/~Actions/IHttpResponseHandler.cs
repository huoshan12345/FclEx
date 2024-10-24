namespace FclEx.Http;

public interface IHttpResponseHandler<T>
{
    OperateResult<T> GetResult(HttpResponse res);
}