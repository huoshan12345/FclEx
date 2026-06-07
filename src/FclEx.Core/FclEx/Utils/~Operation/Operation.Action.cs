namespace FclEx.Utils;

partial class Operation
{
    public static IAction<T> Action<T>(Func<CancellationToken, Task<OperationResult<T>>> execute)
    {
        Check.NotNull(execute);
        return new OperationAction<T>(t => ExecuteAsync(() => execute(t)));
    }

    public static IAction<T> Action<T>(Func<CancellationToken, T> execute)
    {
        Check.NotNull(execute);
        return new OperationAction<T>(t => ExecuteAsync(() => execute(t)));
    }

    public static IAction<T> Action<T>(Func<CancellationToken, Task<T>> execute)
    {
        Check.NotNull(execute);
        return new OperationAction<T>(t => ExecuteAsync(() => execute(t)));
    }

    public static IAction<T> Action<T>(Func<CancellationToken, OperationResult<T>> execute)
    {
        Check.NotNull(execute);
        return new OperationAction<T>(t => ExecuteAsync(() => execute(t)));
    }

    public static IAction<Unit> Action(Action<CancellationToken> execute)
    {
        Check.NotNull(execute);
        return new OperationAction(t => ExecuteAsync(() => execute(t)));
    }

    public static IAction<Unit> Action(Func<CancellationToken, Task> execute)
    {
        Check.NotNull(execute);
        return new OperationAction(t => ExecuteAsync(() => execute(t)));
    }

    public static IAction<Unit> Action(Func<CancellationToken, OperationResult> execute)
    {
        Check.NotNull(execute);
        return new OperationAction(t => ExecuteAsync(() => execute(t)));
    }

    public static IAction<Unit> Action(Func<CancellationToken, Task<OperationResult>> execute)
    {
        Check.NotNull(execute);
        return new OperationAction(t => ExecuteAsync(() => execute(t)));
    }

    public static IAction<T> SuccessAction<T>(T value, TimeSpan elapsed = default)
    {
        return new SuccessAction<T>(value, elapsed);
    }

    public static IAction<Unit> SuccessAction(TimeSpan elapsed = default)
    {
        return new SuccessAction<Unit>(Unit.Default, elapsed);
    }

    public static IAction<T> ErrorAction<T>(string error, TimeSpan elapsed = default)
    {
        return new ErrorAction<T>(error, elapsed);
    }

    public static IAction<T> ErrorAction<T>(Exception exception, TimeSpan elapsed = default)
    {
        return new ErrorAction<T>(exception, elapsed);
    }

    public static IAction<Unit> ErrorAction(string error, TimeSpan elapsed = default)
    {
        return new ErrorAction<Unit>(error, elapsed);
    }

    public static IAction<Unit> ErrorAction(Exception exception, TimeSpan elapsed = default)
    {
        return new ErrorAction<Unit>(exception, elapsed);
    }
}
