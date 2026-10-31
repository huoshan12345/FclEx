namespace FclEx.Extensions.TaskExtensions;

public partial class WaitAsyncTests
{
    public enum WaitOverload
    {
        TaskCancellation,
        TaskTimeout,
        TaskTimeoutAndCancellation,
        GenericCancellation,
        GenericTimeout,
        GenericTimeoutAndCancellation,
    }

    public static TheoryData<WaitOverload> AllOverloads => new()
    {
        WaitOverload.TaskCancellation,
        WaitOverload.TaskTimeout,
        WaitOverload.TaskTimeoutAndCancellation,
        WaitOverload.GenericCancellation,
        WaitOverload.GenericTimeout,
        WaitOverload.GenericTimeoutAndCancellation,
    };

    public static TheoryData<WaitOverload> CancellationOverloads => new()
    {
        WaitOverload.TaskCancellation,
        WaitOverload.TaskTimeoutAndCancellation,
        WaitOverload.GenericCancellation,
        WaitOverload.GenericTimeoutAndCancellation,
    };

    public static TheoryData<WaitOverload> TimeoutOverloads => new()
    {
        WaitOverload.TaskTimeout,
        WaitOverload.TaskTimeoutAndCancellation,
        WaitOverload.GenericTimeout,
        WaitOverload.GenericTimeoutAndCancellation,
    };

    public static IEnumerable<object[]> OverloadsAndCompletionStates()
    {
        foreach (var overload in (WaitOverload[])Enum.GetValues(typeof(WaitOverload)))
        {
            yield return [overload, false];
            yield return [overload, true];
        }
    }

    // Modern targets call the BCL instance methods; net472 calls the FclEx backfills.
    // Keep the same assertions on every target so the BCL also validates our expectations.
    private static Task Wait(Task<int> task, WaitOverload overload, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var effectiveTimeout = timeout ?? Timeout.InfiniteTimeSpan;
        return overload switch
        {
            WaitOverload.TaskCancellation => ((Task)task).WaitAsync(cancellationToken),
            WaitOverload.TaskTimeout => ((Task)task).WaitAsync(effectiveTimeout, CancellationToken.None),
            WaitOverload.TaskTimeoutAndCancellation => ((Task)task).WaitAsync(effectiveTimeout, cancellationToken),
            WaitOverload.GenericCancellation => task.WaitAsync(cancellationToken),
            WaitOverload.GenericTimeout => task.WaitAsync(effectiveTimeout, CancellationToken.None),
            WaitOverload.GenericTimeoutAndCancellation => task.WaitAsync(effectiveTimeout, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(overload)),
        };
    }

    private static TaskCompletionSource<int> CreateSource() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static async Task AwaitWithWatchdog(Task task)
    {
        using var watchdog = new CancellationTokenSource();
        var delay = Task.Delay(TimeSpan.FromSeconds(5), watchdog.Token);
        try
        {
            Assert.Same(task, await Task.WhenAny(task, delay));
            await task;
        }
        finally
        {
            await watchdog.CancelAsync();
        }
    }

    private static async Task AssertSuccessfulWait(Task wait, WaitOverload overload)
    {
        await AwaitWithWatchdog(wait);
        Assert.Equal(TaskStatus.RanToCompletion, wait.Status);
        if (overload is WaitOverload.GenericCancellation or WaitOverload.GenericTimeout or WaitOverload.GenericTimeoutAndCancellation)
            Assert.Equal(42, await (Task<int>)wait);
    }
}
