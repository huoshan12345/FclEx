using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace FclEx.EfCore;

public static class DatabaseFacadeExtensions
{
    /// <summary>
    /// Asynchronously executes the specified SQL and returns the first column of the first row in the result set.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> used to obtain the underlying connection.</param>
    /// <param name="sql">The SQL statement to execute.</param>
    /// <param name="parameters">Optional parameters to add to the command.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// The first column of the first row in the result set cast to <typeparamref name="T"/>.
    /// Returns <c>default</c> if the result is <see langword="null"/> or <see cref="DBNull"/>.
    /// </returns>
    public static async Task<T?> ExecuteScalarRawAsync<T>(this DatabaseFacade database, string sql,
        IEnumerable<IDbDataParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        var connection = database.GetDbConnection();
        var commandTimeout = database.GetCommandTimeout();
        var commandParameters = parameters?.ToArray();
        var strategy = database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            var shouldClose = connection.State != ConnectionState.Open;
            try
            {
                if (shouldClose)
                    await connection.OpenAsync(cancellationToken);

                await using var command = connection.CreateCommand();
                command.Transaction = database.CurrentTransaction?.GetDbTransaction();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;

                if (commandTimeout is { } timeout)
                    command.CommandTimeout = timeout;

                if (commandParameters != null)
                {
                    foreach (var parameter in commandParameters)
                        command.Parameters.Add(parameter);
                }

                try
                {
                    var result = await command.ExecuteScalarAsync(cancellationToken);

                    switch (result)
                    {
                        case null or DBNull:
                            return default;
                        case T t:
                            return t;
                    }

                    var type = typeof(T).UnwrapNullable();

                    if (type == typeof(Guid))
                        return (T)(object)Guid.Parse(result.ToString()!);

                    return Convert.ChangeType<T>(result);
                }
                finally
                {
                    command.Parameters.Clear();
                }
            }
            finally
            {
                if (shouldClose && connection.State != ConnectionState.Closed)
                    await connection.CloseAsync();
            }
        });
    }
}
