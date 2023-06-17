namespace FclEx.Dapper;

public static class DbCommandExtensions
{
    public static async Task<object?> ExecuteScalarAsync(this IDbCommand command, CancellationToken cancellationToken = default)
    {
        return command is DbCommand dbCommand
            ? await dbCommand.ExecuteScalarAsync(cancellationToken)
            : command.ExecuteScalar();
    }

    public static Task<int> ExecuteNonQueryAsync(this IDbCommand command, CancellationToken cancellationToken = default)
    {
        return command is DbCommand dbCommand
            ? dbCommand.ExecuteNonQueryAsync(cancellationToken)
            : Task.FromResult(command.ExecuteNonQuery());
    }
}