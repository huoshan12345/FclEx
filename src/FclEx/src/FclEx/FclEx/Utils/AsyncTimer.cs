namespace FclEx.Utils;

public class AsyncTimer
{
    private readonly Func<CancellationToken, Task> _action;
    private readonly TimeSpan _dueTime;
    private readonly TimeSpan _period;
    private readonly CancellationToken _cancellationToken;
    private readonly Task _task;
    private readonly ILogger _logger;

    public AsyncTimer(Func<CancellationToken, Task> action, TimeSpan dueTime, TimeSpan period, ILoggerFactory? loggerFactory = null, CancellationToken cancellationToken = default)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _dueTime = dueTime;
        _period = period;
        _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<AsyncTimer>();
        _cancellationToken = cancellationToken;
        _task = Task.Run(Run, default);
    }

    private async Task Delay(TimeSpan timeSpan)
    {
        if (timeSpan > TimeSpan.Zero)
            await Task.Delay(timeSpan, _cancellationToken);
    }

    private async Task Run()
    {
        await Delay(_dueTime);
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                var task = _action(_cancellationToken);
                await task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to run action due to {ex.Message}");
            }
            await Delay(_period);
        }
    }
}