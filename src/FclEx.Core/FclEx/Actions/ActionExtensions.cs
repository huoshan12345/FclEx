namespace FclEx.Actions;

public static partial class ActionExtensions
{
    /// <summary>
    /// Maps any successful value to <see cref="Unit"/>.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <returns>An action that keeps failures and maps success to <see cref="Unit"/>.</returns>
    public static IAction<Unit> ToUnit<T>(this IAction<T> action)
    {
        return action.MapValue(m => default(Unit));
    }

    /// <summary>
    /// Maps the successful value of an action.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="T2">The mapped value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="map">The mapper invoked only when the source action succeeds.</param>
    /// <returns>An action that contains the mapped value, or the original failure.</returns>
    public static IAction<T2> MapValue<T, T2>(this IAction<T> action, Func<T, T2> map)
    {
        return new MapValueAction<T, T2>(action, map);
    }

    /// <summary>
    /// Maps the successful value to an operation result.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="T2">The mapped value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="map">The mapper invoked only when the source action succeeds.</param>
    /// <returns>An action that contains the mapped result, or the original failure.</returns>
    public static IAction<T2> MapResult<T, T2>(this IAction<T> action, Func<T, OperationResult<T2>> map)
    {
        return new MapResultAction<T, T2>(action, map);
    }

    /// <summary>
    /// Maps the exception of a failed action.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="map">The exception mapper invoked only when the source action fails.</param>
    /// <returns>An action that keeps success values and replaces failures.</returns>
    public static IAction<T> MapError<T>(this IAction<T> action, Func<Exception, Exception> map)
    {
        Check.NotNull(map);
        return action.ThenResultIf(m => m.IsError, m => new ErrorAction<T>(map(m.Exception!)));
    }

    /// <summary>
    /// Maps the message of the exception of a failed action.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="map">The message mapper invoked only when the source action fails.</param>
    /// <returns>An action that keeps success values and replaces failure messages.</returns>
    public static IAction<T> MapErrorMessage<T>(this IAction<T> action, Func<string, string> map)
    {
        Check.NotNull(map);
        return action.MapError(e => e.SetMessage(map(e.Message)));
    }

    /// <summary>
    /// Converts a successful value into a failure.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="errorFunc">Creates the exception from the successful value.</param>
    /// <returns>An action that fails after the source action succeeds.</returns>
    public static IAction<T> Reject<T>(this IAction<T> action, Func<T, Exception> errorFunc)
    {
        Check.NotNull(errorFunc);
        return action.Then(t => ErrorAction.Create<T>(errorFunc(t)));
    }

    /// <summary>
    /// Converts a successful value into a failure message.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="errorFunc">Creates the failure message from the successful value.</param>
    /// <returns>An action that fails after the source action succeeds.</returns>
    public static IAction<T> Reject<T>(this IAction<T> action, Func<T, string> errorFunc)
    {
        Check.NotNull(errorFunc);
        return action.Then(t => new ErrorAction<T>(errorFunc(t)));
    }

    /// <summary>
    /// Fails the action when a successful value matches the condition.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="condition">The condition evaluated only for successful values.</param>
    /// <param name="errorFunc">Creates the exception when <paramref name="condition"/> returns <see langword="true"/>.</param>
    /// <returns>An action that rejects matching successful values.</returns>
    public static IAction<T> RejectIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, Exception> errorFunc)
    {
        Check.NotNull(condition);
        Check.NotNull(errorFunc);

        return action.Then<T, T>(t => condition(t)
            ? ErrorAction.Create<T>(errorFunc(t))
            : SuccessAction.Create(t));
    }

    /// <summary>
    /// Fails the action with a message when a successful value matches the condition.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="condition">The condition evaluated only for successful values.</param>
    /// <param name="errorFunc">Creates the failure message when <paramref name="condition"/> returns <see langword="true"/>.</param>
    /// <returns>An action that rejects matching successful values.</returns>
    public static IAction<T> RejectIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, string> errorFunc)
    {
        Check.NotNull(condition);
        Check.NotNull(errorFunc);

        return action.Then<T, T>(t => condition(t)
            ? ErrorAction.Create<T>(errorFunc(t))
            : SuccessAction.Create(t));
    }

    /// <summary>
    /// Invokes a callback with the operation result and preserves that result.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="resultAction">The callback invoked for both success and failure results.</param>
    /// <returns>An action that preserves the source result unless the callback fails.</returns>
    /// <remarks>The callback elapsed time is added to the returned result.</remarks>
    public static IAction<T> OnResult<T>(this IAction<T> action, Func<OperationResult<T>, Task> resultAction)
    {
        Check.NotNull(resultAction);
        return action.ThenResult(r => Operation.ExecuteAsync(() => resultAction(r))
            .ThenResult(x => x.Then(_ => r.Elapsed(x.Elapsed))));
    }

    /// <summary>
    /// Invokes a callback with the operation result and preserves that result.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="resultAction">The callback invoked for both success and failure results.</param>
    /// <returns>An action that preserves the source result unless the callback fails.</returns>
    public static IAction<T> OnResult<T>(this IAction<T> action, Action<OperationResult<T>> resultAction)
    {
        Check.NotNull(resultAction);
        return action.OnResult(r =>
        {
            resultAction(r);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Invokes a callback when the operation result matches the condition.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="condition">The condition evaluated against the full operation result.</param>
    /// <param name="resultAction">The callback invoked when the condition matches.</param>
    /// <returns>An action that preserves the source result unless the callback fails.</returns>
    public static IAction<T> WhenResult<T>(this IAction<T> action, Func<OperationResult<T>, bool> condition, Func<OperationResult<T>, Task> resultAction)
    {
        Check.NotNull(condition);
        Check.NotNull(resultAction);

        return action.OnResult(r => condition(r) ? resultAction(r) : Task.CompletedTask);
    }

    /// <summary>
    /// Invokes a callback when the operation result matches the condition.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="condition">The condition evaluated against the full operation result.</param>
    /// <param name="resultAction">The callback invoked when the condition matches.</param>
    /// <returns>An action that preserves the source result unless the callback fails.</returns>
    public static IAction<T> WhenResult<T>(this IAction<T> action, Func<OperationResult<T>, bool> condition, Action<OperationResult<T>> resultAction)
    {
        Check.NotNull(resultAction);

        return action.WhenResult(condition, r =>
        {
            resultAction(r);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Invokes a callback when the successful value matches the condition.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="condition">The condition evaluated only for successful values.</param>
    /// <param name="resultAction">The callback invoked when the condition matches.</param>
    /// <returns>An action that preserves the source result unless the callback fails.</returns>
    public static IAction<T> When<T>(this IAction<T> action, Func<T, bool> condition, Func<T, Task> resultAction)
    {
        Check.NotNull(condition);
        Check.NotNull(resultAction);

        return action.WhenResult(r => r.IsSuccess && condition(r.Value), r => resultAction(r.Value!));
    }

    /// <summary>
    /// Invokes a callback when the successful value matches the condition.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="condition">The condition evaluated only for successful values.</param>
    /// <param name="resultAction">The callback invoked when the condition matches.</param>
    /// <returns>An action that preserves the source result unless the callback fails.</returns>
    public static IAction<T> When<T>(this IAction<T> action, Func<T, bool> condition, Action<T> resultAction)
    {
        Check.NotNull(resultAction);
        return action.When(condition, t =>
        {
            resultAction(t);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Invokes a callback when the action fails.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="errorAction">The callback invoked only for failed results.</param>
    /// <returns>An action that preserves the source result unless the callback fails.</returns>
    public static IAction<T> OnFailed<T>(this IAction<T> action, Func<OperationResult<T>, Task> errorAction)
    {
        Check.NotNull(errorAction);
        return action.WhenResult(r => r.IsError, r => errorAction(r));
    }

    /// <summary>
    /// Invokes a callback when the action fails.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="errorAction">The callback invoked only for failed results.</param>
    /// <returns>An action that preserves the source result unless the callback fails.</returns>
    public static IAction<T> OnFailed<T>(this IAction<T> action, Action<OperationResult<T>> errorAction)
    {
        Check.NotNull(errorAction);
        return action.WhenResult(r => r.IsError, r => errorAction(r));
    }

    /// <summary>
    /// Invokes a callback with the successful value.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="valueAction">The callback invoked only for successful values.</param>
    /// <returns>An action that preserves the source result unless the callback fails.</returns>
    public static IAction<T> OnValue<T>(this IAction<T> action, Action<T> valueAction)
    {
        Check.NotNull(valueAction);
        return action.WhenResult(r => r.IsSuccess, r => valueAction(r.Value!));
    }

    /// <summary>
    /// Invokes a callback with the successful value.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="valueAction">The callback invoked only for successful values.</param>
    /// <returns>An action that preserves the source result unless the callback fails.</returns>
    public static IAction<T> OnValue<T>(this IAction<T> action, Func<T, Task> valueAction)
    {
        Check.NotNull(valueAction);
        return action.WhenResult(r => r.IsSuccess, r => valueAction(r.Value!));
    }

    /// <summary>
    /// Invokes a callback with the exception when the action fails.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="errorAction">The callback invoked with the failed result's exception.</param>
    /// <returns>An action that preserves the source result unless the callback fails.</returns>
    public static IAction<T> OnException<T>(this IAction<T> action, Func<Exception, Task> errorAction)
    {
        Check.NotNull(errorAction);
        return action.WhenResult(r => r.IsError, r => errorAction(r.Exception!));
    }

    /// <summary>
    /// Invokes a callback with the exception when the action fails.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="errorAction">The callback invoked with the failed result's exception.</param>
    /// <returns>An action that preserves the source result unless the callback fails.</returns>
    public static IAction<T> OnException<T>(this IAction<T> action, Action<Exception> errorAction)
    {
        Check.NotNull(errorAction);
        return action.WhenResult(r => r.IsError, r => errorAction(r.Exception!));
    }

    /// <summary>
    /// Re-executes the action once when the first successful value matches the condition.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="condition">The condition evaluated against the first successful value.</param>
    /// <returns>An action that may execute the source action at most twice.</returns>
    /// <remarks>Failed results are not retried by this method.</remarks>
    public static IAction<T> RetryOnceIf<T>(this IAction<T> action, Func<T, bool> condition)
    {
        Check.NotNull(condition);
        return action.Then(t => condition(t) ? action : SuccessAction.Create(t));
    }

    /// <summary>
    /// Repeats the action until a successful value matches the condition.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The action to repeat.</param>
    /// <param name="until">The condition that stops repetition when it returns <see langword="true"/>.</param>
    /// <param name="delay">The delay between successful attempts that do not satisfy <paramref name="until"/>.</param>
    /// <param name="timeout">The optional total timeout for the repeated action.</param>
    /// <returns>An action that repeats until success satisfies the condition, failure, cancellation, or timeout.</returns>
    /// <remarks>
    /// A nonmatching success repeats and a failure is returned immediately. Caller cancellation produces
    /// a canceled result; expiration of <paramref name="timeout"/> produces an error containing a <see cref="TimeoutException"/>.
    /// </remarks>
    public static IAction<T> RepeatUntil<T>(this IAction<T> action, Func<T, bool> until, TimeSpan delay = default, TimeSpan? timeout = null)
    {
        Check.NotNull(until);

        return Operation.Action<T>(async callerToken =>
        {
            var effectiveTimeout = timeout > TimeSpan.Zero ? timeout : null;
            using var cancellation = callerToken.WithTimeout(effectiveTimeout);

            OperationResult<T> CreateTerminationResult()
            {
                return callerToken.IsCancellationRequested
                    ? Operation.Cancel<T>(new OperationCanceledException(callerToken))
                    : Operation.Error<T>(new TimeoutException($"The repeated action did not complete within {effectiveTimeout}."));
            }

            while (true)
            {
                if (cancellation.IsCancellationRequested)
                    return CreateTerminationResult();

                var result = await action.ExecuteAsync(cancellation.Token);
                if (cancellation.IsCancellationRequested)
                    return CreateTerminationResult();

                if (!result.IsSuccess)
                {
                    return result;
                }

                if (until(result.Value!))
                    return result;

                if (delay <= TimeSpan.Zero)
                    continue;

                try
                {
                    await Task.Delay(delay, cancellation.Token);
                }
                catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                {
                    return CreateTerminationResult();
                }
            }
        });
    }

    /// <summary>
    /// Repeats the action until a successful value matches the condition.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The action to repeat.</param>
    /// <param name="until">The condition that stops repetition when it returns <see langword="true"/>.</param>
    /// <param name="delayInSeconds">The delay between attempts, in seconds.</param>
    /// <param name="timeoutInSeconds">The optional total timeout, in seconds.</param>
    /// <returns>An action that repeats until success satisfies the condition, failure, cancellation, or timeout.</returns>
    public static IAction<T> RepeatUntil<T>(this IAction<T> action, Func<T, bool> until, int delayInSeconds = default, int? timeoutInSeconds = null)
    {
        return action.RepeatUntil(until, TimeSpan.FromSeconds(delayInSeconds), timeoutInSeconds.HasValue ? TimeSpan.FromSeconds(timeoutInSeconds.Value) : null);
    }

    /// <summary>
    /// Chains actions to run one after another.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="actions">The actions to chain in enumeration order.</param>
    /// <returns>A single action that returns the last action's result.</returns>
    /// <exception cref="ArgumentException"><paramref name="actions"/> is empty.</exception>
    /// <remarks>An empty sequence is rejected because a successful <c>default(T)</c> cannot be represented for every <typeparamref name="T"/>.</remarks>
    public static IAction<T> Chain<T>(this IEnumerable<IAction<T>> actions)
    {
        IAction<T>? result = null;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var action in actions)
        {
            result = result is null
                ? action
                : result.Then(_ => action);
        }
        return result ?? throw new ArgumentException("The actions sequence is empty.", nameof(actions));
    }

    /// <summary>
    /// Executes the action and discards its value.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="token">The cancellation token passed to the action.</param>
    /// <returns>The operation result without the successful value.</returns>
    public static Task<OperationResult> RunAsync<T>(this IAction<T> action, CancellationToken token = default)
    {
        return action.ExecuteAsync(token).WithoutValue();
    }

    // NOTE: help value type cast to interface.
    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="token">The cancellation token passed to the action.</param>
    /// <returns>The action result.</returns>
    public static Task<OperationResult<T>> ExecuteAsync<T>(this IAction<T> action, CancellationToken token = default)
    {
        return action.ExecuteAsync(token);
    }

    /// <summary>
    /// Executes the action with retries.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="retryCount">The number of retries after the first attempt.</param>
    /// <param name="token">The cancellation token passed to each attempt.</param>
    /// <returns>The first successful result, or the last failed result.</returns>
    public static Task<OperationResult<T>> ExecuteAsync<T>(this IAction<T> action, int retryCount, CancellationToken token)
    {
        return action.ExecuteAsync(retryCount, null, null, token);
    }

    /// <summary>
    /// Executes the action with retries controlled by a result condition.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="retryCount">The number of retries after the first attempt.</param>
    /// <param name="retryCondition">Returns <see langword="true"/> to retry a failed result; any other value stops retrying.</param>
    /// <param name="token">The cancellation token passed to each attempt.</param>
    /// <returns>The first successful result, or the last failed result.</returns>
    public static Task<OperationResult<T>> ExecuteAsync<T>(this IAction<T> action, int retryCount, Func<OperationResult<T>, bool?>? retryCondition, CancellationToken token)
    {
        return action.ExecuteAsync(retryCount, retryCondition, null, token);
    }

    /// <summary>
    /// Executes the action with retries, retry filtering, and optional delay.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="retryCount">The number of retries after the first attempt.</param>
    /// <param name="retryCondition">Returns <see langword="true"/> to retry a failed result; any other value stops retrying.</param>
    /// <param name="sleepDurationProvider">Provides the delay before each retry attempt.</param>
    /// <param name="token">The cancellation token passed to each attempt and delay.</param>
    /// <returns>The first successful result, or the last failed result.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="retryCount"/> is negative.</exception>
    public static async Task<OperationResult<T>> ExecuteAsync<T>(this IAction<T> action,
        int retryCount,
        Func<OperationResult<T>, bool?>? retryCondition = null,
        Func<int, TimeSpan>? sleepDurationProvider = null,
        CancellationToken token = default)
    {
        if (retryCount < 0)
            throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "Retry count cannot be negative.");

        var result = Operation.Error<T>("not started");
        var watch = ValueStopwatch.StartNew();
        for (var attempt = 0; ; attempt++)
        {
            result = await action.ExecuteAsync(token)
                .ThenResult(m => m.Elapsed(watch.GetElapsedTime()));

            if (result.IsSuccess)
                return result;

            if (attempt == retryCount)
                return result;

            if (retryCondition is not null)
            {
                var condition = retryCondition(result);
                if (condition != true)
                    return result;
            }

            if (sleepDurationProvider is null)
                continue;

            var sleepDuration = sleepDurationProvider.Invoke(attempt + 1);
            if (sleepDuration > TimeSpan.Zero)
                await Task.Delay(sleepDuration, token);
        }
    }

    /// <summary>
    /// Combines actions into one action that runs them in series.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="actions">The actions to execute in enumeration order.</param>
    /// <returns>An action that returns all successful values in order.</returns>
    /// <remarks>Execution stops at the first failed action.</remarks>
    public static IAction<T[]> CombineInSeries<T>(this IEnumerable<IAction<T>> actions)
    {
        return SeriesAction.Create(actions);
    }

    /// <summary>
    /// Combines actions into one action that runs them in parallel.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="actions">The actions to execute concurrently.</param>
    /// <returns>An action that returns all successful values in input order.</returns>
    /// <remarks>All actions are started before failures are inspected.</remarks>
    public static IAction<T[]> CombineInParallel<T>(this IEnumerable<IAction<T>> actions)
    {
        return ParallelAction.Create(actions);
    }
}
