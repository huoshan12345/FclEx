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

        var tasks = Enumerable.Repeat(Task.Run(action), times);
        return Task.WhenAll(tasks);
    }

    public static Task Repeat(Action action, int times)
    {
        Check.NotNull(action);
        Check.NotLessThan(times, 0);

        var tasks = Enumerable.Repeat(Task.Run(action), times);
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

    public static Task Delay(int seconds, CancellationToken token = default)
    {
        return Delay(TimeSpan.FromSeconds(seconds), token);
    }

    public static Task DelayMilli(int milliSeconds, CancellationToken token = default)
    {
        return Delay(TimeSpan.FromMilliseconds(milliSeconds), token);
    }

    public static async Task Delay(TimeSpan span, CancellationToken token = default)
    {
        if (span.Ticks <= 0)
            return;
        try
        {
            await Task.Delay(span, token);
        }
        catch (TaskCanceledException) { }
    }

#if !NET5_0_OR_GREATER
    // https://stackoverflow.com/a/22078975/4255140
    public static async Task<TResult> WaitAsync<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource();
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
        if (completedTask == task)
        {
            cts.Cancel();
            return await task;  // Very important in order to propagate exceptions
        }
        else
        {
            throw new TimeoutException("The operation has timed out.");
        }
    }

    public static async Task WaitAsync(this Task task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource();
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
        if (completedTask == task)
        {
            cts.Cancel();
            await task;  // Very important in order to propagate exceptions
        }
        else
        {
            throw new TimeoutException("The operation has timed out.");
        }
    }
#endif

    public static Task<TResult> Run<TResult>(Func<Task<TResult>> task, TimeSpan? timeout = null)
    {
        return timeout is { } time
            ? Task.Run(task).WaitAsync(time)
            : task();
    }

    public static Task<TResult> Run<TResult>(Func<ValueTask<TResult>> task, TimeSpan? timeout = null)
    {
        return Run((Func<Task<TResult>>)(async () => await task()), timeout);
    }

    public static Task Run(Func<Task> task, TimeSpan? timeout = null)
    {
        return timeout is { } time
            ? Task.Run(task).WaitAsync(time)
            : task();
    }

    public static Task Run(Func<ValueTask> task, TimeSpan? timeout = null)
    {
        return Run((Func<Task>)(async () => await task()), timeout);
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
                await ValueTaskWithResultToTask(value, type);
                return GetTaskResult(value, type);
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

    // value should be Task<T> or ValueTask<T>
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