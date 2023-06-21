namespace FclEx.Http;

public interface IHttpResponseHandler<T>
{
    // ReSharper disable once UnusedParameter.Global
    OperateResult<T> GetResult(HttpResponse res);
}