namespace FclEx.Dapper;

public static class DbCommandExtensions
{
    /// <summary>
    /// Executes a scalar command asynchronously when supported, otherwise checks cancellation and executes synchronously.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The token used to cancel native asynchronous execution or checked before fallback execution.</param>
    /// <returns>The first column of the first result row, or <see langword="null"/> when no result is returned.</returns>
    public static async Task<object?> ExecuteScalarAsync(this IDbCommand command, CancellationToken cancellationToken = default)
    {
        if (command is DbCommand dbCommand)
            return await dbCommand.ExecuteScalarAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        return command.ExecuteScalar();
    }

    /// <summary>
    /// Executes a non-query command asynchronously when supported, otherwise checks cancellation and executes synchronously.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The token used to cancel native asynchronous execution or checked before fallback execution.</param>
    /// <returns>The affected row count.</returns>
    public static Task<int> ExecuteNonQueryAsync(this IDbCommand command, CancellationToken cancellationToken = default)
    {
        if (command is DbCommand dbCommand)
            return dbCommand.ExecuteNonQueryAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(command.ExecuteNonQuery());
    }
}
