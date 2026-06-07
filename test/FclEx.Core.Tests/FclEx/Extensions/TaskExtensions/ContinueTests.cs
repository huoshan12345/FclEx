namespace FclEx.Extensions.TaskExtensions;

public class ContinueTests
{
    private const string TaskCanceledExceptionMessage = "A task was canceled.";

    private static Task CreateCanceledTask()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        return Task.FromCanceled(cts.Token);
    }

    private static Task<T> CreateCanceledTask<T>()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        return Task.FromCanceled<T>(cts.Token);
    }

    private static async Task CreateFailedTask(string message)
    {
        await Task.Yield();
        throw new InvalidOperationException(message);
    }

    private static async Task<T> CreateFailedTask<T>(string message)
    {
        await Task.Yield();
        throw new InvalidOperationException(message);
    }

    [Fact]
    public async Task Then_Task_Func_Task_T_Succeeds()
    {
        var task = Task.CompletedTask;

        const int number = 42;
        var result = await task.Then(async () =>
        {
            await Task.Yield();
            return number;
        });

        Assert.Equal(number, result);
    }

    [Fact]
    public async Task Then_Task_Func_Task_T_TaskCancels()
    {
        var task = CreateCanceledTask();
        var action = CreateCanceledTask<int>();
        var result = await Assert.ThrowsAsync<TaskCanceledException>(() => task.Then(() => action));

        Assert.Equal(task, result.Task);
        Assert.Equal(TaskCanceledExceptionMessage, result.Message);
    }

    [Fact]
    public async Task Then_Task_Func_Task_T_TaskFails()
    {
        const string taskMessage = "Task failed";
        const string actionMessage = "Action failed";
        var task = CreateFailedTask(taskMessage);
        var action = CreateFailedTask<int>(actionMessage);
        var result = await Assert.ThrowsAsync<InvalidOperationException>(() => task.Then(() => action));

        Assert.Equal(taskMessage, result.Message);
        Assert.Contains(nameof(CreateFailedTask), result.StackTrace);
    }

    [Fact]
    public async Task Then_Task_Func_Task_T_ActionCancels()
    {
        var task = Task.CompletedTask;
        var action = CreateCanceledTask<int>();
        var result = await Assert.ThrowsAsync<TaskCanceledException>(() => task.Then(() => action));

#if NET5_0_OR_GREATER
        Assert.Equal(action, result.Task); // they are not equal in .net framework
#endif
        Assert.Equal(TaskCanceledExceptionMessage, result.Message);
    }

    [Fact]
    public async Task Then_Task_Func_Task_T_ActionFails()
    {
        const string actionMessage = "Action failed";
        var task = Task.CompletedTask;
        var action = CreateFailedTask<int>(actionMessage);
        var result = await Assert.ThrowsAsync<InvalidOperationException>(() => task.Then(() => action));

        Assert.Equal(actionMessage, result.Message);
        Assert.Contains(nameof(CreateFailedTask), result.StackTrace);
    }


    [Fact]
    public async Task Then_Task_T_Func_T_Task_TResult_Succeeds()
    {
        const int taskResult = 42;
        var actionResult = taskResult.ToString();
        var task = Task.FromResult(taskResult);

        var result = await task.Then(async value =>
        {
            await Task.Yield();
            return actionResult;
        });

        Assert.Equal(actionResult, result);
    }

    [Fact]
    public async Task Then_Task_T_Func_T_Task_TResult_TaskCancels()
    {
        var task = CreateCanceledTask<int>();
        var action = CreateCanceledTask<string>();
        var result = await Assert.ThrowsAsync<TaskCanceledException>(() => task.Then(value => action));

        Assert.Equal(task, result.Task);
        Assert.Equal(TaskCanceledExceptionMessage, result.Message);
    }

    [Fact]
    public async Task Then_Task_T_Func_T_Task_TResult_TaskFails()
    {
        const string taskMessage = "Task failed";
        const string actionMessage = "Action failed";
        var task = CreateFailedTask<int>(taskMessage);
        var action = CreateFailedTask<string>(actionMessage);
        var result = await Assert.ThrowsAsync<InvalidOperationException>(() => task.Then(value => action));

        Assert.Equal(taskMessage, result.Message);
        Assert.Contains(nameof(CreateFailedTask), result.StackTrace);
    }

    [Fact]
    public async Task Then_Task_T_Func_T_Task_TResult_ActionCancels()
    {
        var task = Task.FromResult(42);
        var action = CreateCanceledTask<string>();
        var result = await Assert.ThrowsAsync<TaskCanceledException>(() => task.Then(value => action));

#if NET5_0_OR_GREATER
        Assert.Equal(action, result.Task); // they are not equal in .net framework
#endif
        Assert.Equal("A task was canceled.", result.Message);
    }

    [Fact]
    public async Task Then_Task_T_Func_T_Task_TResult_ActionFails()
    {
        const string actionMessage = "Action failed";
        var task = Task.FromResult(42);
        var action = CreateFailedTask<string>(actionMessage);
        var result = await Assert.ThrowsAsync<InvalidOperationException>(() => task.Then(value => action));

        Assert.Equal(actionMessage, result.Message);
        Assert.Contains(nameof(CreateFailedTask), result.StackTrace);
    }


    [Fact]
    public async Task Then_Task_T_Func_T_Task_Succeeds()
    {
        const int number = 42;
        var task = Task.FromResult(number);

        var result = await task.Then(async value =>
        {
            await Task.Yield();
        });

        Assert.Equal(number, result);
    }

    [Fact]
    public async Task Then_Task_T_Func_T_Task_TaskCancels()
    {
        var task = CreateCanceledTask<int>();
        var action = CreateCanceledTask();
        var result = await Assert.ThrowsAsync<TaskCanceledException>(() => task.Then(value => action));

        Assert.Equal(task, result.Task);
        Assert.Equal(TaskCanceledExceptionMessage, result.Message);
    }

    [Fact]
    public async Task Then_Task_T_Func_T_Task_TaskFails()
    {
        const string taskMessage = "Task failed";
        const string actionMessage = "Action failed";
        var task = CreateFailedTask<int>(taskMessage);
        var action = CreateFailedTask(actionMessage);
        var result = await Assert.ThrowsAsync<InvalidOperationException>(() => task.Then(value => action));

        Assert.Equal(taskMessage, result.Message);
        Assert.Contains(nameof(CreateFailedTask), result.StackTrace);
    }

    [Fact]
    public async Task Then_Task_T_Func_T_Task_ActionCancels()
    {
        var task = Task.FromResult(42);
        var action = CreateCanceledTask();
        var result = await Assert.ThrowsAsync<TaskCanceledException>(() => task.Then(value => action));

        Assert.Equal(action, result.Task);
        Assert.Equal("A task was canceled.", result.Message);
    }

    [Fact]
    public async Task Then_Task_T_Func_T_Task_ActionFails()
    {
        const string actionMessage = "Action failed";
        var task = Task.FromResult(42);
        var action = CreateFailedTask(actionMessage);
        var result = await Assert.ThrowsAsync<InvalidOperationException>(() => task.Then(value => action));

        Assert.Equal(actionMessage, result.Message);
        Assert.Contains(nameof(CreateFailedTask), result.StackTrace);
    }


    [Fact]
    public async Task Catch_ShouldReturnResult_WhenTaskSucceeds()
    {
        var task = Task.FromResult(42);
        var result = await task.Catch(ex => Task.FromResult(-1));
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Catch_ShouldInvokeAction_WhenTaskFaults()
    {
        var task = Task.FromException<int>(new InvalidOperationException("Test exception"));
        var result = await task.Catch(ex => Task.FromResult(99));
        Assert.Equal(99, result);
    }

    [Fact]
    public async Task Catch_ShouldInvokeAction_WhenTaskIsCanceled()
    {
        var cts = new CancellationTokenSource();
        var task = Task.Run(() =>
        {
            cts.Cancel();
            cts.Token.ThrowIfCancellationRequested();
            return 0;
        }, cts.Token);

        var result = await task.Catch(ex => Task.FromResult(88));
        Assert.Equal(88, result);
    }

    [Fact]
    public async Task Catch_ShouldRethrowException_WhenActionThrows()
    {
        var task = Task.FromException<int>(new InvalidOperationException("Test exception"));

        await Assert.ThrowsAsync<ApplicationException>(async () =>
            await task.Catch<int>(ex => throw new ApplicationException("Action failed")));
    }

    [Fact]
    public async Task Catch_ShouldHandleCancellationAndRecover_WhenActionResolves()
    {
        var cts = new CancellationTokenSource();
        var task = Task.Run(() =>
        {
            cts.Cancel();
            cts.Token.ThrowIfCancellationRequested();
            return 0;
        }, cts.Token);

        var result = await task.Catch(ex => Task.FromResult(77));
        Assert.Equal(77, result);
    }
}