namespace FclEx.Actions;

public class CommonAction
{
    public static CommonAction<T> Create<T>(Func<CancellationToken, T> func, bool executeSafely = true)
    {
        return new(t => Operate.CreateSuccess(func(t)), executeSafely);
    }

    public static CommonAction<T> Create<T>(Func<CancellationToken, Task<T>> func, bool executeSafely = true)
    {
        return new(async t => Operate.CreateSuccess(await func(t).IgnoreSyncContext()), executeSafely);
    }

    public static CommonAction<T> Create<T>(Func<CancellationToken, OperateResult<T>> func, bool executeSafely = true)
    {
        return new(t => func(t).ToTask(), executeSafely);
    }

    public static CommonAction<T> Create<T>(Func<CancellationToken, Task<OperateResult<T>>> func, bool executeSafely = true)
    {
        return new(func, executeSafely);
    }

    public static VoidCommonAction Create(Action<CancellationToken> func, bool executeSafely = true)
    {
        return new(t =>
        {
            func(t);
            return Operate.CreateSuccess(default(Unit)).ToTask();
        }, executeSafely);
    }

    public static VoidCommonAction Create(Func<CancellationToken, Task> func, bool executeSafely = true)
    {
        return new(async t =>
        {
            await func(t).IgnoreSyncContext();
            return Operate.CreateSuccess(default(Unit));
        }, executeSafely);
    }

    public static VoidCommonAction Create(Func<CancellationToken, OperateResult> func, bool executeSafely = true)
    {
        return new(t => func(t).ToTask(), executeSafely);
    }

    public static VoidCommonAction Create(Func<CancellationToken, Task<OperateResult>> func, bool executeSafely = true)
    {
        return new(async t => await func(t).IgnoreSyncContext(), executeSafely);
    }
}

public readonly struct CommonAction<T> : IAction<T>
{
    private readonly bool _executeSafely;
    private readonly Func<CancellationToken, Task<OperateResult<T>>> _func;

    public CommonAction(Func<CancellationToken, Task<OperateResult<T>>> func, bool executeSafely)
    {
        _executeSafely = executeSafely;
        _func = Check.NotNull(func);
    }

    public Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        var func = _func;
        return _executeSafely
            ? Operate.ExecuteAsync(() => func(token))
            : func(token);
    }
}

public readonly struct VoidCommonAction : IAction<Unit>
{
    private readonly bool _executeSafely;
    private readonly Func<CancellationToken, Task<OperateResult>> _func;

    public VoidCommonAction(Func<CancellationToken, Task<OperateResult>> func, bool executeSafely)
    {
        _executeSafely = executeSafely;
        _func = Check.NotNull(func);
    }

    public Task<OperateResult<Unit>> ExecuteAsync(CancellationToken token = default)
    {
        var func = _func;
        return _executeSafely
            ? Operate.ExecuteAsync(() => func(token))
            : func(token);
    }
}