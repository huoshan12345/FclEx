namespace FclEx.Helpers;

public enum TaskType
{
    /// <summary>
    /// Represents types that are neither <see cref="Task"/> nor <see cref="ValueTask"/>
    /// </summary>
    NonTask,
    /// <summary>
    /// Represents types like <see cref="Task"/>
    /// </summary>
    VoidTask,
    /// <summary>
    /// Represents types like <see cref="Task{T}"/>
    /// </summary>
    TaskWithResult,
    /// <summary>
    /// Represents types like <see cref="ValueTask"/>
    /// </summary>
    VoidValueTask,
    /// <summary>
    /// Represents types like <see cref="ValueTask{T}"/>
    /// </summary>
    ValueTaskWithResult,
}

public static class TaskHelper
{
    public static Task<TResult[]> Repeat<TResult>(Func<TResult> action, int times)
    {
        Check.NotNull(action);
        Check.NotLessThan(times, 0);

        var tasks = Enumerable.Range(0, times).Select(_ => Task.Run(action));
        return Task.WhenAll(tasks);
    }

    public static Task Repeat(Action action, int times)
    {
        Check.NotNull(action);
        Check.NotLessThan(times, 0);

        var tasks = Enumerable.Range(0, times).Select(_ => Task.Run(action));
        return Task.WhenAll(tasks);
    }

    public static Task<TResult[]> Repeat<TResult>(Func<Task<TResult>> action, int times)
    {
        Check.NotNull(action);
        Check.NotLessThan(times, 0);

        var tasks = Enumerable.Repeat(action, times).Select(m => m());
        return Task.WhenAll(tasks);
    }

    public static Task Repeat(Func<Task> action, int times)
    {
        Check.NotNull(action);
        Check.NotLessThan(times, 0);

        var tasks = Enumerable.Repeat(action, times).Select(m => m());
        return Task.WhenAll(tasks);
    }

#if !NET6_0_OR_GREATER
    public static async Task<T> WaitAsync<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        task = Check.NotNull(task);

        if (task.IsCompleted || cancellationToken == default)
            return await task.NoCapture();

        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        using (cancellationToken.Register((m, t) => m.TrySetCanceled(t), tcs))
        {
            var winner = await Task.WhenAny(task, tcs.Task).NoCapture();

            if (winner != task)
                return await winner.NoCapture();
        }

        return await task.NoCapture();
    }

    public static async Task WaitAsync(this Task task, CancellationToken cancellationToken)
    {
        task = Check.NotNull(task);

        if (task.IsCompleted || cancellationToken == default)
        {
            await task.NoCapture();
            return;
        }

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        using (cancellationToken.Register((m, t) => m.TrySetCanceled(t), tcs))
        {
            var winner = await Task.WhenAny(task, tcs.Task).NoCapture();

            if (winner != task)
            {
                await winner.NoCapture();
                return;
            }
        }

        await task.NoCapture();
    }

    public static async Task<TResult> WaitAsync<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
        {
            return await task.NoCapture();
        }

        using var timeoutSource = new CancellationTokenSource(timeout);
        try
        {
            return await task.WaitAsync(timeoutSource.Token).NoCapture();
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == timeoutSource.Token)
        {
            throw new TimeoutException("The operation did not complete within the specified timeout.", ex);
        }
    }

    public static async Task WaitAsync(this Task task, TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
        {
            await task.NoCapture();
            return;
        }

        using var timeoutSource = new CancellationTokenSource(timeout);
        try
        {
            await task.WaitAsync(timeoutSource.Token).NoCapture();
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == timeoutSource.Token)
        {
            throw new TimeoutException("The operation did not complete within the specified timeout.", ex);
        }
    }
#endif

    public static async Task<TResult> RunAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, TimeSpan? timeout = null)
    {
        if (timeout is not { } timeoutValue || timeoutValue <= TimeSpan.Zero)
        {
            return await RunAsync(operation, CancellationToken.None).NoCapture();
        }

        using var timeoutSource = new CancellationTokenSource(timeoutValue);
        var token = timeoutSource.Token;
        try
        {
            return await RunAsync(operation, token).NoCapture();
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == timeoutSource.Token)
        {
            throw new TimeoutException("The operation did not complete within the specified timeout.", ex);
        }
    }

    public static Task<TResult> RunAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken)
    {
        return cancellationToken == default
            ? operation(cancellationToken)
            : Task.Run(() => operation(cancellationToken), cancellationToken).WaitAsync(cancellationToken);
    }

    public static async Task RunAsync(Func<CancellationToken, Task> operation, TimeSpan? timeout = null)
    {
        if (timeout is not { } timeoutValue || timeoutValue <= TimeSpan.Zero)
        {
            await RunAsync(operation, CancellationToken.None).NoCapture();
            return;
        }

        using var timeoutSource = new CancellationTokenSource(timeoutValue);
        var token = timeoutSource.Token;
        try
        {
            await RunAsync(operation, token).NoCapture();
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == timeoutSource.Token)
        {
            throw new TimeoutException("The operation did not complete within the specified timeout.", ex);
        }
    }

    public static Task RunAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken)
    {
        return cancellationToken == default
            ? operation(cancellationToken)
            : Task.Run(() => operation(cancellationToken), cancellationToken).WaitAsync(cancellationToken);
    }

    private static readonly Type TypeOfVoidTaskResult = Type.GetType("System.Threading.Tasks.VoidTaskResult", true)!;
    private static readonly Type TypeOfTaskOfVoidTaskResult = typeof(Task<>).MakeGenericType(TypeOfVoidTaskResult);

    private static readonly ConcurrentDictionary<Type, TaskType> _taskTypes = new();
    private static readonly ConcurrentDictionary<Type, Func<object, object>> _resultFuncCache = new();
    private static readonly ConcurrentDictionary<Type, Func<object, Task>> _asTaskFuncCache = new();

    private static TaskType GetTaskType(Type type)
    {
        if (type == typeof(Task) || type == TypeOfTaskOfVoidTaskResult)
            return TaskType.VoidTask;

        if (type == typeof(ValueTask))
            return TaskType.VoidValueTask;

        if (type.IsGenericType && type.GetGenericTypeDefinition() is var genericType)
        {
            if (genericType == typeof(Task<>))
                return TaskType.TaskWithResult;

            if (genericType == typeof(ValueTask<>))
                return TaskType.ValueTaskWithResult;
        }

        return TaskType.NonTask;
    }

    public static async Task<object?> AwaitObject(object? value)
    {
        if (value is null)
            return null;

        var type = value.GetType();
        var taskType = _taskTypes.GetOrAdd(type, GetTaskType);

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (taskType)
        {
            case TaskType.VoidTask:
            case TaskType.VoidValueTask:
            {
                await (dynamic)value;
                return null;
            }
            case TaskType.TaskWithResult:
            {
                await (Task)value;
                return GetTaskResult(value, type);
            }
            case TaskType.ValueTaskWithResult:
            {
                // NOTE: we can not use "result = (object)(await (dynamic)value)" here,
                // because when T of ValueTask<T> is non-public, we will get a RuntimeBinderException that says "'System.ValueType' does not contain a definition for 'GetAwaiter'".
                var task = ValueTaskWithResultToTask(value, type);
                await task.NoCapture();
                var resultTaskType = typeof(Task<>).MakeGenericType(type.GetGenericArguments()[0]);
                return GetTaskResult(task, resultTaskType);
            }
            default:
            {
                return value;
            }
        }
    }

    // value should be ValueTask<T>
    private static Task ValueTaskWithResultToTask(object value, Type type)
    {
        // NOTE: if we use "await (dynamic)value" to await a ValueTask<T> when T is non-public, we will get an RuntimeBinderException that says
        // 'System.ValueType' does not contain a definition for 'GetAwaiter'.
        // So we have to convert ValueTask<T> to Task and then await it.
        // Please fix this logic if there is a better solution.
        var func = _asTaskFuncCache.GetOrAdd(type, k =>
        {
            var parameter = Expression.Parameter(typeof(object), "type");
            var convertedParameter = Expression.Convert(parameter, k);
            var method = k.GetRequiredMethod(nameof(ValueTask<int>.AsTask));
            var property = Expression.Call(convertedParameter, method);
            var convertedProperty = Expression.Convert(property, typeof(Task));
            var exp = Expression.Lambda<Func<object, Task>>(convertedProperty, parameter);
            return exp.Compile();
        });
        return func(value);
    }

    // mark this method as "internal" for testing.
    internal static Func<object, object> CreateFuncToGetTaskResult(Type typeInfo)
    {
        var parameter = Expression.Parameter(typeof(object), "type");
        var convertedParameter = Expression.Convert(parameter, typeInfo);
        var property = Expression.Property(convertedParameter, nameof(Task<int>.Result));
        var convertedProperty = Expression.Convert(property, typeof(object));
        var exp = Expression.Lambda<Func<object, object>>(convertedProperty, parameter);
        return exp.Compile();
    }

    // value should be Task<T>.
    private static object GetTaskResult(object value, Type type)
    {
        // There are several ways to get the value of "Result" of Task<T> or ValueTask<T> after it is awaited.
        // The Benchmark can be viewed in GetTaskResultBenchmarks.cs in AspectCore.Core.Benchmark.
        // Here is a test result that can be referred to:
        /*
            |                            Method |         Mean |
            |---------------------------------- |-------------:|
            |          GetTaskResult_Reflection |     338.6 ns |
            | GetTaskResult_ReflectionWithCache |     284.9 ns |
            |          GetTaskResult_Expression | 224,786.1 ns |
            | GetTaskResult_ExpressionWithCache |     126.2 ns |
            |        GetTaskResult_AwaitDynamic |     117.1 ns |
        */
        // So we use "ExpressionWithCache" here.
        // Please fix this logic if there is a better solution.
        var func = _resultFuncCache.GetOrAdd(type, k => CreateFuncToGetTaskResult(k));
        return func(value);
    }
}
