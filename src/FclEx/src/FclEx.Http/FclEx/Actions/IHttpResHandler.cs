namespace FclEx.Actions;

public interface IHttpResHandler<T>
{
    OperateResult<T> GetResult(HttpResponse res);
}