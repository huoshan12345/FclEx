using System.Globalization;
using static FclEx.Dapper.DapperHelper;

// ReSharper disable RedundantCast
#pragma warning disable IDE0004

namespace FclEx.Dapper;

internal readonly record struct SqlInfo(string Sql, IReadOnlyList<DbParameter> Params);
internal readonly record struct EntitySqlKey(ISqlAdapter SqlAdapter, EntityMapping Mapping);
internal readonly record struct InsertSqlKey(
    ISqlAdapter SqlAdapter,
    EntityMapping Mapping,
    bool IncludeAutoKey,
    bool ReturnGeneratedKey,
    int RowCount);

/// <summary>
/// Provides execution, provider, and entity-mapping options for an FclEx.Dapper command.
/// </summary>
/// <param name="TimeoutSeconds">The optional command timeout in seconds.</param>
/// <param name="Transaction">The optional local transaction assigned to the command.</param>
/// <param name="SqlAdapter">An optional SQL adapter overriding connection-type resolution.</param>
/// <param name="EntityMappingSource">
/// An optional entity mapping source. <see cref="DapperHelper.DefaultEntityMappingSource"/> is used when omitted.
/// </param>
/// <param name="CancellationToken">The token used to cancel connection opening and command execution.</param>
public readonly record struct CommandInfo(
    int? TimeoutSeconds = null,
    DbTransaction? Transaction = null,
    ISqlAdapter? SqlAdapter = null,
    IEntityMappingSource? EntityMappingSource = null,
    CancellationToken CancellationToken = default);

public static partial class DbConnectionExtensions
{
    // This limit bounds both command size and the row-number dimension of the process-wide INSERT SQL cache.
    private const int DefaultInsertBatchSize = 500;

    // These process-wide caches are reserved for the canonical path: a registered or built-in adapter plus the
    // schema stored in a stable EntityMapping. Per-call schema and adapter overrides are intentionally excluded.
    internal static readonly ConcurrentDictionary<EntitySqlKey, string> GetSqls = new();
    internal static readonly ConcurrentDictionary<EntitySqlKey, string> DeleteSqls = new();
    internal static readonly ConcurrentDictionary<InsertSqlKey, string> InsertSqls = new();

    // Positional parameter names avoid retaining every CLR property name. Rows are bounded by
    // DefaultInsertBatchSize, so this cache grows only to the widest mapped INSERT seen by the process.
    internal static readonly ConcurrentDictionary<(int Column, int Row), string> ParameterNames = new();

    internal static void RemoveSqlCacheEntries(ISqlAdapter sqlAdapter)
    {
        // Adapter registration is a rare configuration change. Remove only entries for the replaced adapter so
        // its instance can be collected without disrupting hot SQL cached for unrelated connection providers.
        foreach (var key in GetSqls.Keys.Where(key => ReferenceEquals(key.SqlAdapter, sqlAdapter)))
            GetSqls.TryRemove(key, out _);
        foreach (var key in DeleteSqls.Keys.Where(key => ReferenceEquals(key.SqlAdapter, sqlAdapter)))
            DeleteSqls.TryRemove(key, out _);
        foreach (var key in InsertSqls.Keys.Where(key => ReferenceEquals(key.SqlAdapter, sqlAdapter)))
            InsertSqls.TryRemove(key, out _);
    }

    private static bool CanUseGlobalSqlCache(string? schema, CommandInfo commandInfo)
    {
        // A null schema selects the schema in the stable mapping. A null adapter override selects either a
        // private built-in adapter or a registered adapter whose replacement invalidates these caches. Mapping sources
        // are required to return stable mappings, so their mapping instances are safe parts of the cache key.
        return schema is null && commandInfo.SqlAdapter is null;
    }

    /// <summary>
    /// Inserts one mapped entity and optionally returns its single database-generated key.
    /// </summary>
    /// <typeparam name="TEntity">The mapped entity type.</typeparam>
    /// <typeparam name="TKey">The generated-key type requested by the caller.</typeparam>
    /// <param name="con">The connection used to execute the insert. A connection opened here is closed before return.</param>
    /// <param name="entity">The entity whose mapped values are inserted.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="returnGeneratedKey">Whether to return the single generated key when one is mapped.</param>
    /// <param name="commandInfo">Command execution, adapter, mapping, transaction, and cancellation options.</param>
    /// <returns>The generated key converted to <typeparamref name="TKey"/> when requested and supported; otherwise the default value.</returns>
    /// <exception cref="OperationCanceledException"><see cref="CommandInfo.CancellationToken"/> is cancelled.</exception>
    public static Task<TKey?> InsertAsync<TEntity, TKey>(
        this DbConnection con,
        TEntity entity,
        string? schema = null,
        bool returnGeneratedKey = true,
        CommandInfo commandInfo = default)
        where TEntity : class
    {
        return InsertCoreAsync<TEntity, TKey>(
            con,
            entity,
            schema,
            returnGeneratedKey,
            false,
            commandInfo);
    }

    /// <summary>
    /// Inserts one mapped entity and returns its single database-generated key as a long.
    /// </summary>
    /// <typeparam name="TEntity">The mapped entity type.</typeparam>
    /// <param name="con">The connection used to execute the insert. A connection opened here is closed before return.</param>
    /// <param name="entity">The entity whose mapped values are inserted.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="returnGeneratedKey">Whether to return the single generated key when one is mapped.</param>
    /// <param name="commandInfo">Command execution, adapter, mapping, transaction, and cancellation options.</param>
    /// <returns>The generated key converted to <see langword="long"/> when requested and supported; otherwise the default value.</returns>
    /// <exception cref="OperationCanceledException"><see cref="CommandInfo.CancellationToken"/> is cancelled.</exception>
    public static Task<long> InsertAsync<TEntity>(
        this DbConnection con,
        TEntity entity,
        string? schema = null,
        bool returnGeneratedKey = true,
        CommandInfo commandInfo = default)
        where TEntity : class
    {
        return con.InsertAsync<TEntity, long>(entity, schema, returnGeneratedKey, commandInfo);
    }

    /// <summary>
    /// Inserts one mapped entity while explicitly supplying its database-generated key values.
    /// </summary>
    /// <typeparam name="TEntity">The mapped entity type.</typeparam>
    /// <param name="con">The connection used to execute the insert. A connection opened here is closed before return.</param>
    /// <param name="entity">The entity containing the generated key values to insert.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="commandInfo">Command execution, adapter, mapping, transaction, and cancellation options.</param>
    /// <returns>A task representing the insert operation.</returns>
    /// <exception cref="DataException">The entity mapping does not contain a database-generated key.</exception>
    /// <exception cref="OperationCanceledException"><see cref="CommandInfo.CancellationToken"/> is cancelled.</exception>
    public static async Task InsertWithExplicitKeysAsync<TEntity>(
        this DbConnection con,
        TEntity entity,
        string? schema = null,
        CommandInfo commandInfo = default)
        where TEntity : class
    {
        await InsertCoreAsync<TEntity, object>(con, entity, schema, false, true, commandInfo);
    }

    private static async Task<TKey?> InsertCoreAsync<TEntity, TKey>(
        DbConnection con,
        TEntity entity,
        string? schema,
        bool returnGeneratedKey,
        bool includeGeneratedKeys,
        CommandInfo commandInfo)
        where TEntity : class
    {
        var mapping = GetEntityMapping(typeof(TEntity), commandInfo.EntityMappingSource);
        if (includeGeneratedKeys && mapping.GeneratedKeys.Count == 0)
            throw new DataException($"Entity '{mapping.EntityType.FullName}' does not have a database-generated key.");

        var shouldReturnGeneratedKey = returnGeneratedKey
                                       && includeGeneratedKeys == false
                                       && mapping.GeneratedKeys.Count == 1;
        var useGlobalSqlCache = CanUseGlobalSqlCache(schema, commandInfo);
        var value = await con.ExecuteAsync(commandInfo, m => GetInsertSql(
            m,
            schema,
            entity,
            mapping,
            shouldReturnGeneratedKey,
            includeGeneratedKeys,
            useGlobalSqlCache), async (a, m) =>
        {
            if (includeGeneratedKeys)
            {
                var tableName = GetTableNameWithSchema(a, schema, mapping);
                await using var x = await a.BeginExplicitIdentityInsertAsync(
                    tableName,
                    m,
                    commandInfo.CancellationToken);
                return await ExecuteCommandAsync(m);
            }

            return await ExecuteCommandAsync(m);

            async Task<object?> ExecuteCommandAsync(DbCommand command)
            {
                if (shouldReturnGeneratedKey)
                    return await command.ExecuteScalarAsync(commandInfo.CancellationToken);

                await command.ExecuteNonQueryAsync(commandInfo.CancellationToken);
                return null;
            }
        });
        return ConvertGeneratedKey<TKey>(value);
    }

    private static TKey? ConvertGeneratedKey<TKey>(object? value)
    {
        if (value is null or DBNull)
            return default;
        if (value is TKey key)
            return key;

        var keyType = Nullable.GetUnderlyingType(typeof(TKey)) ?? typeof(TKey);
        if (keyType.IsEnum)
        {
            if (value is float or double or decimal)
                value = Convert.ChangeType(value, Enum.GetUnderlyingType(keyType), CultureInfo.InvariantCulture);
            return (TKey)Enum.ToObject(keyType, value);
        }

        return (TKey)Convert.ChangeType(value, keyType, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Inserts entities into table asynchronously and returns affected rows.
    /// </summary>
    /// <typeparam name="T">The mapped entity type.</typeparam>
    /// <param name="con">The connection used to execute the batches. A connection opened here is closed before return.</param>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="includeAutoKey">Whether to insert mapped generated keys explicitly.</param>
    /// <param name="commandInfo">Command execution, adapter, mapping, transaction, and cancellation options.</param>
    /// <returns>The total affected rows reported by all batches, or zero for an empty collection.</returns>
    /// <exception cref="NotSupportedException">The mapped row shape cannot be represented by the selected adapter.</exception>
    /// <exception cref="OperationCanceledException"><see cref="CommandInfo.CancellationToken"/> is cancelled.</exception>
    public static async Task<int> BulkInsertAsync<T>(this DbConnection con, IReadOnlyCollection<T> entities, string? schema = null, bool includeAutoKey = false, CommandInfo commandInfo = default)
        where T : class
    {
        if (entities.IsNullOrEmpty())
            return 0;

        var initialState = con.State;
        var sqlAdapter = commandInfo.SqlAdapter ?? GetSqlAdapter(con);
        var mapping = GetEntityMapping(typeof(T), commandInfo.EntityMappingSource);
        var insertProperties = mapping.GetInsertProperties(includeAutoKey);
        if (insertProperties.Count == 0 && entities.Count > 1)
        {
            throw new NotSupportedException(
                $"Bulk insertion of multiple '{mapping.EntityType.FullName}' entities with no insertable properties is not supported.");
        }

        var batchSize = insertProperties.Count == 0
            ? 1
            : Math.Min(DefaultInsertBatchSize, sqlAdapter.GetMaxInsertBatchSize(insertProperties.Count));
        var useGlobalSqlCache = CanUseGlobalSqlCache(schema, commandInfo);

        // Override paths do not enter process-wide caches. A small call-local cache still makes identical full
        // batches share one command text; normally it contains only the full batch size and the final remainder.
        Dictionary<InsertSqlKey, string>? localInsertSqls = useGlobalSqlCache ? null : new();

        async Task<int> ExecuteBatchesAsync()
        {
            if (entities.Count <= batchSize)
                return await ExecuteBatchAsync(entities);

            var affectedRows = 0;
            var batch = new List<T>(batchSize);
            foreach (var entity in entities)
            {
                batch.Add(entity);
                if (batch.Count < batchSize)
                    continue;

                affectedRows += await ExecuteBatchAsync(batch);
                batch.Clear();
            }

            if (batch.Count > 0)
                affectedRows += await ExecuteBatchAsync(batch);

            return affectedRows;
        }

        async Task<int> ExecuteBatchAsync(IReadOnlyCollection<T> batch)
        {
            var (sql, parameters) = GetBulkInsertSql(
                sqlAdapter,
                schema,
                batch,
                mapping,
                includeAutoKey,
                useGlobalSqlCache,
                localInsertSqls);
            using var command = con.CreateCommand(sql, parameters, commandInfo.TimeoutSeconds, commandInfo.Transaction);
            return await command.ExecuteNonQueryAsync(commandInfo.CancellationToken);
        }

        try
        {
            await con.TryOpenAsync(commandInfo.CancellationToken);

            if (includeAutoKey && mapping.GeneratedKeys.Count > 0)
            {
                using var command = con.CreateCommand();
                command.Transaction = commandInfo.Transaction;
                if (commandInfo.TimeoutSeconds is { } timeout)
                {
                    command.CommandTimeout = timeout;
                }

                var tableName = GetTableNameWithSchema(sqlAdapter, schema, mapping);
                await using var scope = await sqlAdapter.BeginExplicitIdentityInsertAsync(
                    tableName,
                    command,
                    commandInfo.CancellationToken);
                return await ExecuteBatchesAsync();
            }

            return await ExecuteBatchesAsync();
        }
        finally
        {
            RestoreInitialConnectionState(con, initialState);
        }
    }

    internal static string GetParameterName(int column, int row)
    {
        return ParameterNames.GetOrAdd((column, row), static key => $"@p{key.Column}_{key.Row}");
    }

    internal static SqlInfo GetBulkInsertSql<T>(
        ISqlAdapter sqlAdapter,
        string? schema,
        IReadOnlyCollection<T> entities,
        EntityMapping mapping,
        bool includeAutoKey,
        bool useGlobalSqlCache,
        IDictionary<InsertSqlKey, string>? localInsertSqls)
    {
        var insertProperties = mapping.GetInsertProperties(includeAutoKey);
        var sql = GetInsertCommandText(
            sqlAdapter,
            schema,
            mapping,
            includeAutoKey,
            false,
            entities.Count,
            useGlobalSqlCache,
            localInsertSqls);
        var paras = new List<DbParameter>(insertProperties.Count * entities.Count);
        foreach (var (i, item) in entities.Index())
        {
            foreach (var (column, property) in insertProperties.Index())
            {
                var paraName = GetParameterName(column, i);
                var value = property.Property.GetValue(item);
                var para = sqlAdapter.CreateParameter(paraName, value, property.StoreTypeName);
                paras.Add(para);
            }
        }

        return new(sql, paras);
    }

    internal static string GetInsertCommandText(
        ISqlAdapter sqlAdapter,
        string? schema,
        EntityMapping mapping,
        bool includeAutoKey,
        bool returnGeneratedKey,
        int rowCount,
        bool useGlobalSqlCache,
        IDictionary<InsertSqlKey, string>? localInsertSqls = null)
    {
        if (rowCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(rowCount));
        if (rowCount > DefaultInsertBatchSize)
            throw new ArgumentOutOfRangeException(nameof(rowCount));
        if (useGlobalSqlCache && schema is not null)
            throw new ArgumentException("A per-call schema override cannot be stored in the global SQL cache.", nameof(schema));

        if (includeAutoKey || mapping.GeneratedKeys.Count != 1)
            returnGeneratedKey = false;

        var key = new InsertSqlKey(sqlAdapter, mapping, includeAutoKey, returnGeneratedKey, rowCount);
        if (useGlobalSqlCache)
        {
            // The global key deliberately omits schema: this branch only represents the schema embedded in the
            // stable mapping. That invariant prevents arbitrary schema strings from becoming permanent keys.
            return InsertSqls.GetOrAdd(key, static cacheKey => CreateInsertCommandText(cacheKey, null));
        }

        if (localInsertSqls is not null && localInsertSqls.TryGetValue(key, out var cachedSql))
            return cachedSql;

        // The local dictionary also omits schema because it belongs to one BulkInsertAsync call, whose schema is
        // fixed. It is discarded with that call and therefore cannot retain either the schema or adapter long term.
        var sql = CreateInsertCommandText(key, schema);
        if (localInsertSqls is not null)
            localInsertSqls[key] = sql;
        return sql;

        static string CreateInsertCommandText(InsertSqlKey key, string? schema)
        {
            var (sqlAdapter, mapping, includeAutoKey, returnGeneratedKey, rowCount) = key;
            var tableName = GetTableNameWithSchema(sqlAdapter, schema, mapping);
            var insertProperties = mapping.GetInsertProperties(includeAutoKey);
            if (insertProperties.Count == 0)
            {
                if (rowCount != 1)
                    throw new NotSupportedException("Multiple default-values rows cannot be represented by the common INSERT command shape.");

                var generatedKeyColumn = returnGeneratedKey
                    ? sqlAdapter.GetQuotedColumnName(mapping.GeneratedKeys[0].ColumnName)
                    : null;
                return sqlAdapter.BuildInsertCommandText(tableName, null, null, generatedKeyColumn);
            }

            var sbColumnList = new StringBuilder(1024);
            foreach (var (_, property, _, isLast) in insertProperties.IndexEx())
            {
                sbColumnList.Append(sqlAdapter.GetQuotedColumnName(property.ColumnName));
                if (isLast == false)
                    sbColumnList.Append(", ");
            }

            var sbParameterList = new StringBuilder(1024);
            for (var i = 0; i < rowCount; i++)
            {
                sbParameterList.Append('(');
                foreach (var (column, _, _, isLast) in insertProperties.IndexEx())
                {
                    var paraName = GetParameterName(column, i);
                    sbParameterList.Append(paraName);

                    if (isLast == false)
                        sbParameterList.Append(", ");
                }
                sbParameterList.Append(')');
                if (i < rowCount - 1)
                    sbParameterList.Append(',').Append(Environment.NewLine);
            }

            var generatedKeyColumnName = returnGeneratedKey
                ? sqlAdapter.GetQuotedColumnName(mapping.GeneratedKeys[0].ColumnName)
                : null;
            return sqlAdapter.BuildInsertCommandText(
                tableName,
                sbColumnList.ToString(),
                sbParameterList.ToString(),
                generatedKeyColumnName);
        }
    }

    internal static SqlInfo GetInsertSql<T>(
        ISqlAdapter sqlAdapter,
        string? schema,
        T entity,
        EntityMapping mapping,
        bool returnGeneratedKey,
        bool includeGeneratedKeys,
        bool useGlobalSqlCache)
    {
        var sql = GetInsertCommandText(
            sqlAdapter,
            schema,
            mapping,
            includeGeneratedKeys,
            returnGeneratedKey,
            1,
            useGlobalSqlCache);
        var insertProperties = mapping.GetInsertProperties(includeGeneratedKeys);
        var paras = new List<DbParameter>(insertProperties.Count);
        foreach (var (column, property) in insertProperties.Index())
        {
            var paraName = GetParameterName(column, 0);
            var value = property.Property.GetValue(entity);
            paras.Add(sqlAdapter.CreateParameter(paraName, value, property.StoreTypeName));
        }

        return new(sql, paras);
    }

    /// <summary>
    /// Gets one mapped entity by its single key.
    /// </summary>
    /// <typeparam name="T">The mapped entity type.</typeparam>
    /// <param name="connection">The connection used to execute the query. Dapper restores its initial open/closed state.</param>
    /// <param name="id">The key value to find.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="commandInfo">Command execution, adapter, mapping, transaction, and cancellation options.</param>
    /// <returns>The matching entity, or <see langword="null"/> when no row matches.</returns>
    /// <exception cref="DataException">The mapping does not define exactly one key.</exception>
    /// <exception cref="OperationCanceledException"><see cref="CommandInfo.CancellationToken"/> is cancelled.</exception>
    public static Task<T?> GetAsync<T>(this DbConnection connection, object id, string? schema = null, CommandInfo commandInfo = default)
    {
        var adapter = commandInfo.SqlAdapter ?? GetSqlAdapter(connection);
        var mapping = GetEntityMapping(typeof(T), commandInfo.EntityMappingSource);
        var key = new EntitySqlKey(adapter, mapping);
        var sql = CanUseGlobalSqlCache(schema, commandInfo)
            ? GetSqls.GetOrAdd(key, static cacheKey => CreateGetSql(cacheKey, null))
            : CreateGetSql(key, schema);
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        return connection.QueryFirstOrDefaultAsync<T?>(new CommandDefinition(
            sql,
            dynParams,
            commandInfo.Transaction,
            commandInfo.TimeoutSeconds,
            cancellationToken: commandInfo.CancellationToken));

        static string CreateGetSql(EntitySqlKey key, string? schema)
        {
            var (sqlAdapter, mapping) = key;
            var keyProperty = GetSingleKey(mapping);
            var tableName = GetTableNameWithSchema(sqlAdapter, schema, mapping);
            var keyName = sqlAdapter.GetQuotedColumnName(keyProperty.ColumnName);
            var selectColumns = string.Join(", ", mapping.Properties.Select(property =>
                $"{sqlAdapter.GetQuotedColumnName(property.ColumnName)} AS {sqlAdapter.GetQuotedColumnName(property.Property.Name)}"));
            return $"SELECT {selectColumns} FROM {tableName} WHERE {keyName} = @id";
        }
    }

    /// <summary>
    /// Deletes one mapped entity by its single key.
    /// </summary>
    /// <typeparam name="T">The mapped entity type.</typeparam>
    /// <param name="con">The connection used to execute the delete. Dapper restores its initial open/closed state.</param>
    /// <param name="id">The key value to delete.</param>
    /// <param name="schema">An optional schema overriding the schema in the entity mapping.</param>
    /// <param name="commandInfo">Command execution, adapter, mapping, transaction, and cancellation options.</param>
    /// <returns>The affected row count.</returns>
    /// <exception cref="DataException">The mapping does not define exactly one key.</exception>
    /// <exception cref="OperationCanceledException"><see cref="CommandInfo.CancellationToken"/> is cancelled.</exception>
    public static Task<int> DeleteAsync<T>(this DbConnection con, object id, string? schema = null, CommandInfo commandInfo = default)
    {
        var adapter = commandInfo.SqlAdapter ?? GetSqlAdapter(con);
        var mapping = GetEntityMapping(typeof(T), commandInfo.EntityMappingSource);
        var key = new EntitySqlKey(adapter, mapping);
        var sql = CanUseGlobalSqlCache(schema, commandInfo)
            ? DeleteSqls.GetOrAdd(key, static cacheKey => CreateDeleteSql(cacheKey, null))
            : CreateDeleteSql(key, schema);
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        return con.ExecuteAsync(new CommandDefinition(
            sql,
            dynParams,
            commandInfo.Transaction,
            commandInfo.TimeoutSeconds,
            cancellationToken: commandInfo.CancellationToken));

        static string CreateDeleteSql(EntitySqlKey key, string? schema)
        {
            var (sqlAdapter, mapping) = key;
            var keyProperty = GetSingleKey(mapping);
            var tableName = GetTableNameWithSchema(sqlAdapter, schema, mapping);
            var keyName = sqlAdapter.GetQuotedColumnName(keyProperty.ColumnName);
            return $"DELETE FROM {tableName} WHERE {keyName} = @id";
        }
    }

    private static PropertyMapping GetSingleKey(EntityMapping mapping)
    {
        var keys = mapping.Keys;
        if (keys.Count > 1)
            throw new DataException($"Only entities with a single mapped key are supported. Key count: {keys.Count}");
        if (keys.Count == 0)
            throw new DataException("Only entities with a mapped key are supported.");
        return keys[0];
    }

    internal static Task<T> ExecuteAsync<T>(this DbConnection con, CommandInfo commandInfo, Func<ISqlAdapter, SqlInfo> sqlFunc, Func<DbCommand, Task<T>> func)
    {
        return con.ExecuteAsync(commandInfo, sqlFunc, (_, m) => func(m));
    }

    internal static async Task<T> ExecuteAsync<T>(this DbConnection con, CommandInfo commandInfo, Func<ISqlAdapter, SqlInfo> sqlFunc, Func<ISqlAdapter, DbCommand, Task<T>> func)
    {
        var initialState = con.State;
        try
        {
            var adapter = commandInfo.SqlAdapter ?? GetSqlAdapter(con);
            var (sql, paras) = sqlFunc(adapter);
            var cmd = con.CreateCommand(sql, paras, commandInfo.TimeoutSeconds, commandInfo.Transaction);
            await con.TryOpenAsync(commandInfo.CancellationToken);
            return await func(adapter, cmd);
        }
        finally
        {
            RestoreInitialConnectionState(con, initialState);
        }
    }

    private static void RestoreInitialConnectionState(DbConnection connection, ConnectionState initialState)
    {
        // The extensions borrow their caller-owned connection. Restore Closed only when this operation observed
        // Closed before opening it; an already-open connection remains owned and managed by the caller.
        if (initialState == ConnectionState.Closed && connection.State != ConnectionState.Closed)
            connection.Close();
    }

    public static DbCommand CreateCommand(this DbConnection con, string sql, IEnumerable<DbParameter>? paras = null, int? timeoutSeconds = null, DbTransaction? transaction = null)
    {
        var command = con.CreateCommand();
        command.CommandText = sql;
        command.Transaction = transaction;
        foreach (var item in paras.EmptyIfNull())
        {
            command.Parameters.Add(item);
        }
        if (timeoutSeconds is { } timeout)
        {
            command.CommandTimeout = timeout;
        }
        return command;
    }
}
