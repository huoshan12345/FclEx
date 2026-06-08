namespace FclEx.Utils;

public static partial class Operation
{
    /// <summary>
    /// Creates an action from an asynchronous operation-result factory.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="execute">The function executed when the action runs.</param>
    /// <returns>An action whose execution is wrapped by <see cref="ExecuteAsync{T}(Func{Task{OperationResult{T}}}, TimeSpan?)"/>.</returns>
    public static IAction<T> Action<T>(Func<CancellationToken, Task<OperationResult<T>>> execute)
    {
        Check.NotNull(execute);
        return new OperationAction<T>(t => ExecuteAsync(() => execute(t)));
    }

    /// <summary>
    /// Creates an action from a synchronous value factory.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="execute">The function executed when the action runs.</param>
    /// <returns>An action whose execution converts returned values and thrown exceptions into operation results.</returns>
    public static IAction<T> Action<T>(Func<CancellationToken, T> execute)
    {
        Check.NotNull(execute);
        return new OperationAction<T>(t => ExecuteAsync(() => execute(t)));
    }

    /// <summary>
    /// Creates an action from an asynchronous value factory.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="execute">The function executed when the action runs.</param>
    /// <returns>An action whose execution converts returned values and thrown exceptions into operation results.</returns>
    public static IAction<T> Action<T>(Func<CancellationToken, Task<T>> execute)
    {
        Check.NotNull(execute);
        return new OperationAction<T>(t => ExecuteAsync(() => execute(t)));
    }

    /// <summary>
    /// Creates an action from a synchronous operation-result factory.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="execute">The function executed when the action runs.</param>
    /// <returns>An action whose execution returns the produced operation result after flattening outer execution timing.</returns>
    public static IAction<T> Action<T>(Func<CancellationToken, OperationResult<T>> execute)
    {
        Check.NotNull(execute);
        return new OperationAction<T>(t => ExecuteAsync(() => execute(t)));
    }

    /// <summary>
    /// Creates a unit action from a synchronous operation.
    /// </summary>
    /// <param name="execute">The operation executed when the action runs.</param>
    /// <returns>An action whose execution converts completion and thrown exceptions into operation results.</returns>
    public static IAction<Unit> Action(Action<CancellationToken> execute)
    {
        Check.NotNull(execute);
        return new OperationAction(t => ExecuteAsync(() => execute(t)));
    }

    /// <summary>
    /// Creates a unit action from an asynchronous operation.
    /// </summary>
    /// <param name="execute">The operation executed when the action runs.</param>
    /// <returns>An action whose execution converts completion and thrown exceptions into operation results.</returns>
    public static IAction<Unit> Action(Func<CancellationToken, Task> execute)
    {
        Check.NotNull(execute);
        return new OperationAction(t => ExecuteAsync(() => execute(t)));
    }

    /// <summary>
    /// Creates a unit action from a synchronous operation-result factory.
    /// </summary>
    /// <param name="execute">The function executed when the action runs.</param>
    /// <returns>An action whose execution returns the produced operation result after flattening outer execution timing.</returns>
    public static IAction<Unit> Action(Func<CancellationToken, OperationResult> execute)
    {
        Check.NotNull(execute);
        return new OperationAction(t => ExecuteAsync(() => execute(t)));
    }

    /// <summary>
    /// Creates a unit action from an asynchronous operation-result factory.
    /// </summary>
    /// <param name="execute">The function executed when the action runs.</param>
    /// <returns>An action whose execution returns the produced operation result after flattening outer execution timing.</returns>
    public static IAction<Unit> Action(Func<CancellationToken, Task<OperationResult>> execute)
    {
        Check.NotNull(execute);
        return new OperationAction(t => ExecuteAsync(() => execute(t)));
    }

    /// <summary>
    /// Creates an action that always succeeds with a value.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="value">The value returned by the action.</param>
    /// <param name="elapsed">The elapsed time stored in the action result.</param>
    public static IAction<T> SuccessAction<T>(T value, TimeSpan elapsed = default)
    {
        return new SuccessAction<T>(value, elapsed);
    }

    /// <summary>
    /// Creates a unit action that always succeeds.
    /// </summary>
    /// <param name="elapsed">The elapsed time stored in the action result.</param>
    public static IAction<Unit> SuccessAction(TimeSpan elapsed = default)
    {
        return new SuccessAction<Unit>(Unit.Default, elapsed);
    }

    /// <summary>
    /// Creates an action that always fails with a message.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="error">The error message.</param>
    /// <param name="elapsed">The elapsed time stored in the action result.</param>
    public static IAction<T> ErrorAction<T>(string error, TimeSpan elapsed = default)
    {
        return new ErrorAction<T>(error, elapsed);
    }

    /// <summary>
    /// Creates an action that always fails with an exception.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="exception">The exception stored in the action result.</param>
    /// <param name="elapsed">The elapsed time stored in the action result.</param>
    public static IAction<T> ErrorAction<T>(Exception exception, TimeSpan elapsed = default)
    {
        return new ErrorAction<T>(exception, elapsed);
    }

    /// <summary>
    /// Creates a unit action that always fails with a message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="elapsed">The elapsed time stored in the action result.</param>
    public static IAction<Unit> ErrorAction(string error, TimeSpan elapsed = default)
    {
        return new ErrorAction<Unit>(error, elapsed);
    }

    /// <summary>
    /// Creates a unit action that always fails with an exception.
    /// </summary>
    /// <param name="exception">The exception stored in the action result.</param>
    /// <param name="elapsed">The elapsed time stored in the action result.</param>
    public static IAction<Unit> ErrorAction(Exception exception, TimeSpan elapsed = default)
    {
        return new ErrorAction<Unit>(exception, elapsed);
    }
}
