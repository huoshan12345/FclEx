namespace FclEx.Utils;

partial class Operation
{
    public static OperationAction<T> Action<T>(Func<CancellationToken, T> func)
    {
        return new(t => ExecuteAsync(() => func(t)));
    }

    public static OperationAction<T> Action<T>(Func<CancellationToken, Task<T>> func)
    {
        return new(t => ExecuteAsync(() => func(t)));
    }

    public static OperationAction<T> Action<T>(Func<CancellationToken, OperationResult<T>> func)
    {
        return new(t => ExecuteAsync(() => func(t)));
    }

    public static OperationAction<T> Action<T>(Func<CancellationToken, Task<OperationResult<T>>> func)
    {
        return new(t => ExecuteAsync(() => func(t)));
    }

    public static OperationAction Action(Action<CancellationToken> func)
    {
        return new(t => ExecuteAsync(() => func(t)));
    }

    public static OperationAction Action(Func<CancellationToken, Task> func)
    {
        return new(t => ExecuteAsync(() => func(t)));
    }

    public static OperationAction Action(Func<CancellationToken, OperationResult> func)
    {
        return new(t => ExecuteAsync(() => func(t)));
    }

    public static OperationAction Action(Func<CancellationToken, Task<OperationResult>> func)
    {
        return new(t => ExecuteAsync(() => func(t)));
    }

    public static SuccessAction<T> SuccessAction<T>(T obj, TimeSpan timeSpan = default)
    {
        return new(obj, timeSpan);
    }

    public static SuccessAction SuccessAction(TimeSpan timeSpan = default)
    {
        return new(Unit.Default, timeSpan);
    }

    public static ErrorAction<T> ErrorAction<T>(string error, TimeSpan timeSpan = default)
    {
        return new(error, timeSpan);
    }

    public static ErrorAction<T> ErrorAction<T>(Exception ex, TimeSpan timeSpan = default)
    {
        return new(ex, timeSpan);
    }
    
    public static ErrorAction ErrorAction(string error, TimeSpan timeSpan = default)
    {
        return new(error, timeSpan);
    }

    public static ErrorAction ErrorAction(Exception ex, TimeSpan timeSpan = default)
    {
        return new(ex, timeSpan);
    }
}