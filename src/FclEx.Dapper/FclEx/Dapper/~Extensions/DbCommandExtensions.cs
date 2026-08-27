namespace FclEx.Dapper;

/// <summary>
/// Provides asynchronous execution helpers for ADO.NET commands exposed as <see cref="IDbCommand"/>.
/// </summary>
/// <remarks>
/// <see cref="DbCommand"/> instances use provider-native asynchronous APIs. Other implementations run their
/// synchronous operation on the thread pool and cannot be cancelled after that operation starts.
/// </remarks>
public static class DbCommandExtensions
{
    /// <summary>
    /// Executes a scalar command asynchronously when supported, otherwise checks cancellation and executes synchronously.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The token used to cancel native asynchronous execution or checked before fallback execution.</param>
    /// <returns>The first column of the first result row, or <see langword="null"/> when no result is returned.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is cancelled before completion.</exception>
    public static Task<object?> ExecuteScalarAsync(this IDbCommand command, CancellationToken cancellationToken = default)
    {
        if (command is DbCommand dbCommand)
            return dbCommand.ExecuteScalarAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run<object?>(() => command.ExecuteScalar(), cancellationToken);
    }

    /// <summary>
    /// Executes a non-query command asynchronously when supported, otherwise checks cancellation and executes synchronously.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The token used to cancel native asynchronous execution or checked before fallback execution.</param>
    /// <returns>The affected row count.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is cancelled before completion.</exception>
    public static Task<int> ExecuteNonQueryAsync(this IDbCommand command, CancellationToken cancellationToken = default)
    {
        if (command is DbCommand dbCommand)
            return dbCommand.ExecuteNonQueryAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() => command.ExecuteNonQuery(), cancellationToken);
    }
}
