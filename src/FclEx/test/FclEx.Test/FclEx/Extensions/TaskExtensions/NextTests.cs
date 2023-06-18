using System.Threading.Tasks;
using Xunit.Abstractions;
#pragma warning disable CS4014

namespace FclEx.Extensions.TaskExtensions;

public class NextTests
{
    private readonly ITestOutputHelper _output;

    public NextTests(ITestOutputHelper output)
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
    public async Task Task_Next_Action()
    {
        {
            var number = 0;
            await Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(100)))
                .Next(() => number++);

            Assert.Equal(1, number);
        }
        {
            var task = Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(100)))
                .Next(() => Throw());

            await Assert.ThrowsAsync<NotSupportedException>(() => task);
        }
    }

    [Fact]
    public async Task Task_Next_Action_Faulted()
    {
        var task = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            throw new InvalidOperationException();
        }).Next(() => Throw());

        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }

    [Fact]
    public async Task Task_Next_Action_Canceled()
    {
        var token = new CancellationToken(true);
        var task = Task.FromCanceled(token);
        var task2 = task.Next(() => Throw());

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(() => task2);
        Assert.Equal(task, ex.Task);
    }

    [Fact]
    public async Task Task_Next_Func_Task()
    {
        {
            var number = 0;
            await Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(100)))
                .Next(() =>
                {
                    number++;
                    return Task.CompletedTask;
                });

            Assert.Equal(1, number);
        }
        {
            var task = Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(100)))
                .Next(() => ThrowTask());

            await Assert.ThrowsAsync<NotSupportedException>(() => task);
        }
    }

    [Fact]
    public async Task Task_Next_Func_Task_Faulted()
    {
        var task = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            throw new InvalidOperationException();
        }).Next(() => ThrowTask());

        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }

    [Fact]
    public async Task Task_Next_Func_Task_Canceled()
    {
        var token = new CancellationToken(true);
        var task = Task.FromCanceled(token);
        var task2 = task.Next(() => ThrowTask());

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(() => task2);
        Assert.Equal(task, ex.Task);
    }

    [Fact]
    public async Task Task_Next_Func_Task_TNext()
    {
        {
            var number = await Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(100)))
                .Next(() => 1.ToTask());

            Assert.Equal(1, number);
        }
        {
            var task = Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(100)))
                .Next(() => ThrowTask<int>());

            Assert.IsAssignableFrom<Task<int>>(task);

            await Assert.ThrowsAsync<NotSupportedException>(() => task);
        }
    }

    [Fact]
    public async Task Task_Next_Func_Task_TNext_Faulted()
    {
        var task = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            throw new InvalidOperationException();
        }).Next(() => ThrowTask<int>());

        Assert.IsAssignableFrom<Task<int>>(task);

        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
    }

    [Fact]
    public async Task Task_Next_Func_Task_TNext_Canceled()
    {
        var token = new CancellationToken(true);
        var task0 = Task.FromCanceled(token);
        var task = task0.Next(() => ThrowTask<int>());

        Assert.IsAssignableFrom<Task<int>>(task);

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(() => task);
        Assert.Equal(task0, ex.Task);
    }

}