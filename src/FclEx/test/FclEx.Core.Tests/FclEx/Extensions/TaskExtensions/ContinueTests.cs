#pragma warning disable CS4014

namespace FclEx.Extensions.TaskExtensions;

public class ContinueTests
{
    private readonly ITestOutputHelper _output;

    public ContinueTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static void Throw()
    {
        throw new NotSupportedException();
    }

    private static Task ThrowTask()
    {
        throw new NotSupportedException();
    }

    private static Task<T> ThrowTask<T>()
    {
        throw new NotSupportedException();
    }

    [Fact]
    public async Task Task_Continue_Action()
    {
        {
            var number = 0;
            await Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(100)))
                .Continue(() => number++);

            Assert.Equal(1, number);
        }
        {
            var task = Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(100)))
                .Continue(() => Throw());

            await Assert.ThrowsAsync<NotSupportedException>(() => task);
        }
    }

    [Fact]
    public async Task Task_Continue_Action_Faulted()
    {
        var task = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            throw new InvalidOperationException();
        }).Continue(() => Throw());

        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }

    [Fact]
    public async Task Task_Continue_Action_Canceled()
    {
        var token = new CancellationToken(true);
        var task = Task.FromCanceled(token);
        var task2 = task.Continue(() => Throw());

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(() => task2);
        Assert.Equal(task, ex.Task);
    }

    [Fact]
    public async Task Task_Continue_Func_Task()
    {
        {
            var number = 0;
            await Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(100)))
                .Continue(() =>
                {
                    number++;
                    return Task.CompletedTask;
                });

            Assert.Equal(1, number);
        }
        {
            var task = Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(100)))
                .Continue(() => ThrowTask());

            await Assert.ThrowsAsync<NotSupportedException>(() => task);
        }
    }

    [Fact]
    public async Task Task_Continue_Func_Task_Faulted()
    {
        var task = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            throw new InvalidOperationException();
        }).Continue(() => ThrowTask());

        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }

    [Fact]
    public async Task Task_Continue_Func_Task_Canceled()
    {
        var token = new CancellationToken(true);
        var task = Task.FromCanceled(token);
        var task2 = task.Continue(() => ThrowTask());

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(() => task2);
        Assert.Equal(task, ex.Task);
    }

    [Fact]
    public async Task Task_Continue_Func_Task_TNext()
    {
        {
            var number = await Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(100)))
                .Continue(() => 1.ToTask());

            Assert.Equal(1, number);
        }
        {
            var task = Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(100)))
                .Continue(() => ThrowTask<int>());

            Assert.IsAssignableFrom<Task<int>>(task);

            await Assert.ThrowsAsync<NotSupportedException>(() => task);
        }
    }

    [Fact]
    public async Task Task_Continue_Func_Task_TNext_Faulted()
    {
        var task = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            throw new InvalidOperationException();
        }).Continue(() => ThrowTask<int>());

        Assert.IsAssignableFrom<Task<int>>(task);

        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }

    [Fact]
    public async Task Task_Continue_Func_Task_TNext_Canceled()
    {
        var token = new CancellationToken(true);
        var task0 = Task.FromCanceled(token);
        var task = task0.Continue(() => ThrowTask<int>());

        Assert.IsAssignableFrom<Task<int>>(task);

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(() => task);
        Assert.Equal(task0, ex.Task);
    }

}