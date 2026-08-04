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
    /// Returns <c>default</c> if the result is <c>null</c> or <see cref="DBNull"/>.
    /// </returns>
    public static async Task<T?> ExecuteScalarRawAsync<T>(this DatabaseFacade database, string sql,
        IEnumerable<IDbDataParameter>? parameters = null, CancellationToken cancellationToken = default)
    {
        var connection = database.GetDbConnection();

        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
            await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();

        try
        {
            command.Transaction = database.CurrentTransaction?.GetDbTransaction();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            if (parameters != null)
            {
                foreach (var p in parameters)
                    command.Parameters.Add(p);
            }

            var strategy = database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
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
            });
        }
        finally
        {
            await command.DisposeAsync();

            if (shouldClose)
                await connection.CloseAsync();
        }

    }
}
