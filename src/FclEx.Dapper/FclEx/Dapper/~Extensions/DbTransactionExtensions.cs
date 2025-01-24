namespace FclEx.Dapper;

public static class DbTransactionExtensions
{
#if NETSTANDARD2_0
    public static Task CommitAsync(this DbTransaction tran)
    {
        tran.Commit();
        return Task.CompletedTask;
    }

    public static Task RollbackAsync(this DbTransaction tran)
    {
        tran.Rollback();
        return Task.CompletedTask;
    }
#endif

    public static async Task TryRollbackAsync(this DbTransaction tran)
    {
        if (tran.Connection is not { State: ConnectionState.Open })
            return;

        await tran.RollbackAsync();
    }

    public static async Task TryRollbackAsync(this IEnumerable<DbTransaction> trans, Exception commitException)
    {
        // Ensure that every transaction will be roll-backed.
        var results = await trans.Select(m => Operation.ExecuteAsync(m.TryRollbackAsync)).WhenAll();
        var errors = results.Where(m => m.Error).ToArray();
        if (errors.Any())
        {
            throw new AggregateException(errors.Select(m => m.Exception!).Append(commitException));
        }
        else commitException.ReThrow();
    }

    /// <summary>
    /// Inserts an entity into table asynchronously. <br/>
    /// Returns identity only if entity has an auto-increment key and includeAutoKey is <see langword="false"/>, otherwise returns <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tran"></param>
    /// <param name="entity"></param>
    /// <param name="schema"></param>
    /// <param name="returnId"></param>
    /// <param name="includeAutoKey"></param>
    /// <param name="timeoutSeconds"></param>
    /// <param name="sqlAdapter"></param>
    /// <returns></returns>
    public static Task<dynamic?> InsertAsync<T>(this DbTransaction tran, T entity, string? schema = null, bool returnId = true, bool includeAutoKey = false, int? timeoutSeconds = null, ISqlAdapter? sqlAdapter = null)
        where T : class
    {
        return tran.Connection!.InsertAsync(entity, schema, returnId, includeAutoKey, new(timeoutSeconds, tran, sqlAdapter));
    }

    /// <summary>
    /// Inserts entities into table asynchronously and returns affected rows.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tran"></param>
    /// <param name="entities"></param>
    /// <param name="schema"></param>
    /// <param name="includeAutoKey"></param>
    /// <param name="timeoutSeconds"></param>
    /// <param name="sqlAdapter"></param>
    /// <returns></returns>
    public static Task<int> BulkInsertAsync<T>(this DbTransaction tran, IReadOnlyCollection<T> entities, string? schema = null, bool includeAutoKey = false, int? timeoutSeconds = null, ISqlAdapter? sqlAdapter = null)
        where T : class
    {
        return tran.Connection!.BulkInsertAsync(entities, schema, includeAutoKey, new(timeoutSeconds, tran, sqlAdapter));
    }

    /// <summary>
    /// Get an entity by id
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tran"></param>
    /// <param name="id"></param>
    /// <param name="schema"></param>
    /// <param name="timeoutSeconds"></param>
    /// <param name="sqlAdapter"></param>
    /// <returns></returns>
    public static Task<T?> GetAsync<T>(this DbTransaction tran, object id, string? schema = null, int? timeoutSeconds = null, ISqlAdapter? sqlAdapter = null)
    {
        return tran.Connection!.GetAsync<T>(id, schema, new(timeoutSeconds, tran, sqlAdapter));
    }

    /// <summary>
    /// Delete an entity by id
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tran"></param>
    /// <param name="id"></param>
    /// <param name="schema"></param>
    /// <param name="timeoutSeconds"></param>
    /// <param name="sqlAdapter"></param>
    /// <returns></returns>
    public static Task<int> DeleteAsync<T>(this DbTransaction tran, object id, string? schema = null, int? timeoutSeconds = null, ISqlAdapter? sqlAdapter = null)
    {
        return tran.Connection!.DeleteAsync<T>(id, schema, new(timeoutSeconds, tran, sqlAdapter));
    }
}