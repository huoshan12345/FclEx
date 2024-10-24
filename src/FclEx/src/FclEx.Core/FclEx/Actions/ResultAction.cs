namespace FclEx.Actions;

public static class ResultAction
{
    public static ResultAction<T> Create<T>(OperateResult<T> result) => new(result);
    public static ResultAction<T> Create<T>(T value, TimeSpan elapsed = default) 
        => new(Operate.CreateSuccess(value, elapsed));
}

public readonly struct ResultAction<T> : IAction<T>
{
    private readonly OperateResult<T> _result;

    public ResultAction(OperateResult<T> result)
    {
        _result = result;
    }

    public Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return _result;
    }
}