namespace FclEx.Actions;

public interface IHttpResHandler<T>
{
    OperateResult<T> GetResult(HttpRes res);
}