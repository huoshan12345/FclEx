namespace FclEx.Actions;

public class CommonAction
{
    public static CommonAction<T> Create<T>(Func<CancellationToken, T> func, bool executeSafely = true)
    {
        return new(t => Operation.Success(func(t)), executeSafely);
    }

    public static CommonAction<T> Create<T>(Func<CancellationToken, Task<T>> func, bool executeSafely = true)
    {
        return new(async t => Operation.Success(await func(t).IgnoreSyncContext()), executeSafely);
    }

    public static CommonAction<T> Create<T>(Func<CancellationToken, OperationResult<T>> func, bool executeSafely = true)
    {
        return new(t => func(t).ToTask(), executeSafely);
    }

    public static CommonAction<T> Create<T>(Func<CancellationToken, Task<OperationResult<T>>> func, bool executeSafely = true)
    {
        return new(func, executeSafely);
    }

    public static VoidCommonAction Create(Action<CancellationToken> func, bool executeSafely = true)
    {
        return new(t =>
        {
            func(t);
            return Operation.Success(default(Unit)).ToTask();
        }, executeSafely);
    }

    public static VoidCommonAction Create(Func<CancellationToken, Task> func, bool executeSafely = true)
    {
        return new(async t =>
        {
            await func(t).IgnoreSyncContext();
            return Operation.Success(default(Unit));
        }, executeSafely);
    }

    public static VoidCommonAction Create(Func<CancellationToken, OperationResult> func, bool executeSafely = true)
    {
        return new(t => func(t).ToTask(), executeSafely);
    }

    public static VoidCommonAction Create(Func<CancellationToken, Task<OperationResult>> func, bool executeSafely = true)
    {
        return new(async t => await func(t).IgnoreSyncContext(), executeSafely);
    }

    public static SuccessAction<T> Success<T>(T obj, TimeSpan timeSpan = default)
    {
        return SuccessAction.Create(obj, timeSpan);
    }

    public static ErrorAction<T> Error<T>(string error, TimeSpan timeSpan = default)
    {
        return ErrorAction.Create<T>(error, timeSpan);
    }

    public static ErrorAction<T> Error<T>(Exception ex, TimeSpan timeSpan = default)
    {
        return ErrorAction.Create<T>(ex, timeSpan);
    }
}

public readonly struct CommonAction<T> : IAction<T>
{
    private readonly bool _executeSafely;
    private readonly Func<CancellationToken, Task<OperationResult<T>>> _func;

    public CommonAction(Func<CancellationToken, Task<OperationResult<T>>> func, bool executeSafely)
    {
        _executeSafely = executeSafely;
        _func = Check.NotNull(func);
    }

    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        var func = _func;
        return _executeSafely
            ? Operation.ExecuteAsync(() => func(token))
            : func(token);
    }
}

public readonly struct VoidCommonAction : IAction<Unit>
{
    private readonly bool _executeSafely;
    private readonly Func<CancellationToken, Task<OperationResult>> _func;

    public VoidCommonAction(Func<CancellationToken, Task<OperationResult>> func, bool executeSafely)
    {
        _executeSafely = executeSafely;
        _func = Check.NotNull(func);
    }

    public Task<OperationResult<Unit>> ExecuteAsync(CancellationToken token = default)
    {
        var func = _func;
        return _executeSafely
            ? Operation.ExecuteAsync(() => func(token))
            : func(token);
    }
}