namespace FclEx.Helpers;

/// <summary>
/// Executes synchronous and asynchronous operations with a fixed retry policy.
/// </summary>
public static class RetryHelper
{
    /// <summary>
    /// Executes an operation until it succeeds, declines another retry, or exhausts the configured retries.
    /// </summary>
    /// <param name="operation">The token-aware operation to execute.</param>
    /// <param name="maxRetryCount">The maximum number of retries after the initial attempt.</param>
    /// <param name="retryDelay">The delay before each retry.</param>
    /// <param name="shouldRetry">
    /// An optional predicate that determines whether a failure is retryable. When omitted, every exception except
    /// <see cref="OperationCanceledException"/> is retryable.
    /// </param>
    /// <param name="cancellationToken">Cancels the operation or a pending retry delay.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxRetryCount"/> or <paramref name="retryDelay"/> is negative.
    /// </exception>
    /// <remarks>
    /// The final operation exception is rethrown with its original stack trace. Exceptions thrown by
    /// <paramref name="shouldRetry"/> propagate directly and do not cause the operation exception to be retried.
    /// </remarks>
    public static void Execute(
        Action<CancellationToken> operation,
        int maxRetryCount = 3,
        TimeSpan retryDelay = default,
        Predicate<Exception>? shouldRetry = null,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(operation);

        Execute(
            token =>
            {
                operation(token);
                return true;
            },
            maxRetryCount,
            retryDelay,
            shouldRetry,
            cancellationToken);
    }

    /// <summary>
    /// Executes a value-producing operation until it succeeds, declines another retry, or exhausts the configured retries.
    /// </summary>
    /// <typeparam name="T">The operation result type.</typeparam>
    /// <param name="operation">The token-aware operation to execute.</param>
    /// <param name="maxRetryCount">The maximum number of retries after the initial attempt.</param>
    /// <param name="retryDelay">The delay before each retry.</param>
    /// <param name="shouldRetry">
    /// An optional predicate that determines whether a failure is retryable. When omitted, every exception except
    /// <see cref="OperationCanceledException"/> is retryable.
    /// </param>
    /// <param name="cancellationToken">Cancels the operation or a pending retry delay.</param>
    /// <returns>The value returned by the first successful attempt.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxRetryCount"/> or <paramref name="retryDelay"/> is negative.
    /// </exception>
    /// <remarks>The final exception is rethrown with its original stack trace.</remarks>
    public static T Execute<T>(
        Func<CancellationToken, T> operation,
        int maxRetryCount = 3,
        TimeSpan retryDelay = default,
        Predicate<Exception>? shouldRetry = null,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(operation, maxRetryCount, retryDelay);

        for (var retryCount = 0; ; retryCount++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return operation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (retryCount >= maxRetryCount || shouldRetry?.Invoke(exception) == false)
                    throw;

                Delay(retryDelay, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Executes an asynchronous operation until it succeeds, declines another retry, or exhausts the configured retries.
    /// </summary>
    /// <param name="operation">The token-aware asynchronous operation to execute.</param>
    /// <param name="maxRetryCount">The maximum number of retries after the initial attempt.</param>
    /// <param name="retryDelay">The delay before each retry.</param>
    /// <param name="shouldRetry">
    /// An optional predicate that determines whether a failure is retryable. When omitted, every exception except
    /// <see cref="OperationCanceledException"/> is retryable.
    /// </param>
    /// <param name="cancellationToken">Cancels the operation or a pending retry delay.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxRetryCount"/> or <paramref name="retryDelay"/> is negative.
    /// </exception>
    /// <remarks>
    /// The final operation exception is rethrown with its original stack trace. Exceptions thrown by
    /// <paramref name="shouldRetry"/> propagate directly and do not cause the operation exception to be retried.
    /// </remarks>
    public static async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        int maxRetryCount = 3,
        TimeSpan retryDelay = default,
        Predicate<Exception>? shouldRetry = null,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(operation);

        await ExecuteAsync(
            async token =>
            {
                await operation(token).NoCapture();
                return true;
            },
            maxRetryCount,
            retryDelay,
            shouldRetry,
            cancellationToken).NoCapture();
    }

    /// <summary>
    /// Executes an asynchronous value-producing operation until it succeeds, declines another retry, or exhausts the configured retries.
    /// </summary>
    /// <typeparam name="T">The operation result type.</typeparam>
    /// <param name="operation">The token-aware asynchronous operation to execute.</param>
    /// <param name="maxRetryCount">The maximum number of retries after the initial attempt.</param>
    /// <param name="retryDelay">The delay before each retry.</param>
    /// <param name="shouldRetry">
    /// An optional predicate that determines whether a failure is retryable. When omitted, every exception except
    /// <see cref="OperationCanceledException"/> is retryable.
    /// </param>
    /// <param name="cancellationToken">Cancels the operation or a pending retry delay.</param>
    /// <returns>The value returned by the first successful attempt.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxRetryCount"/> or <paramref name="retryDelay"/> is negative.
    /// </exception>
    /// <remarks>The final exception is rethrown with its original stack trace.</remarks>
    public static async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        int maxRetryCount = 3,
        TimeSpan retryDelay = default,
        Predicate<Exception>? shouldRetry = null,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(operation, maxRetryCount, retryDelay);

        for (var retryCount = 0; ; retryCount++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation(cancellationToken).NoCapture();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (retryCount >= maxRetryCount || shouldRetry?.Invoke(exception) == false)
                    throw;

                if (retryDelay > TimeSpan.Zero)
                    await Task.Delay(retryDelay, cancellationToken).NoCapture();
            }
        }
    }

    private static void ValidateArguments(Delegate operation, int maxRetryCount, TimeSpan retryDelay)
    {
        Check.NotNull(operation);
        Check.NotLessThan(maxRetryCount, 0);
        Check.NotLessThan(retryDelay, TimeSpan.Zero);
    }

    private static void Delay(TimeSpan retryDelay, CancellationToken cancellationToken)
    {
        if (retryDelay <= TimeSpan.Zero)
            return;

        if (cancellationToken.CanBeCanceled == false)
        {
            Thread.Sleep(retryDelay);
            return;
        }

        if (cancellationToken.WaitHandle.WaitOne(retryDelay))
            cancellationToken.ThrowIfCancellationRequested();
    }
}
