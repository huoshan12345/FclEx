namespace FclEx.Utils;

partial class Operation
{
    public static IAction<T> Action<T>(Func<CancellationToken, Task<OperationResult<T>>> func)
    {
        return new OperationAction<T>(t => ExecuteAsync(() => func(t)));
    }

    public static IAction<T> Action<T>(Func<CancellationToken, T> func)
    {
        return new OperationAction<T>(t => ExecuteAsync(() => func(t)));
    }

    public static IAction<T> Action<T>(Func<CancellationToken, Task<T>> func)
    {
        return new OperationAction<T>(t => ExecuteAsync(() => func(t)));
    }

    public static IAction<T> Action<T>(Func<CancellationToken, OperationResult<T>> func)
    {
        return new OperationAction<T>(t => ExecuteAsync(() => func(t)));
    }

    public static IAction<Unit> Action(Action<CancellationToken> func)
    {
        return new OperationAction(t => ExecuteAsync(() => func(t)));
    }

    public static IAction<Unit> Action(Func<CancellationToken, Task> func)
    {
        return new OperationAction(t => ExecuteAsync(() => func(t)));
    }

    public static IAction<Unit> Action(Func<CancellationToken, OperationResult> func)
    {
        return new OperationAction(t => ExecuteAsync(() => func(t)));
    }

    public static IAction<Unit> Action(Func<CancellationToken, Task<OperationResult>> func)
    {
        return new OperationAction(t => ExecuteAsync(() => func(t)));
    }

    public static IAction<T> SuccessAction<T>(T obj, TimeSpan timeSpan = default)
    {
        return new SuccessAction<T>(obj, timeSpan);
    }

    public static IAction<Unit> SuccessAction(TimeSpan timeSpan = default)
    {
        return new SuccessAction<Unit>(Unit.Default, timeSpan);
    }

    public static IAction<T> ErrorAction<T>(string error, TimeSpan timeSpan = default)
    {
        return new ErrorAction<T>(error, timeSpan);
    }

    public static IAction<T> ErrorAction<T>(Exception ex, TimeSpan timeSpan = default)
    {
        return new ErrorAction<T>(ex, timeSpan);
    }

    public static IAction<Unit> ErrorAction(string error, TimeSpan timeSpan = default)
    {
        return new ErrorAction<Unit>(error, timeSpan);
    }

    public static IAction<Unit> ErrorAction(Exception ex, TimeSpan timeSpan = default)
    {
        return new ErrorAction<Unit>(ex, timeSpan);
    }
}