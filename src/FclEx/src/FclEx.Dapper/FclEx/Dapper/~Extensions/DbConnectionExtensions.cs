using static FclEx.Dapper.DapperHelper;

// ReSharper disable RedundantCast
#pragma warning disable IDE0004

namespace FclEx.Dapper;

internal readonly record struct SqlInfo(string Sql, IReadOnlyList<DbParameter> Params);
internal readonly record struct EntitySqlKey(ISqlAdapter SqlAdapter, string? Schema, Type EntityType);
internal readonly record struct InsertColumnsKey(ISqlAdapter SqlAdapter, string? Schema, Type EntityType, bool IncludeAutoKey);
internal readonly record struct InsertValuesKey(Type EntityType, int Count, bool IncludeAutoKey);

public readonly record struct CommandInfo(int? TimeoutSeconds = null, DbTransaction? Transaction = null, ISqlAdapter? SqlAdapter = null);

public static partial class DbConnectionExtensions
{
    // ReSharper disable once InconsistentNaming
    internal static readonly ConcurrentDictionary<EntitySqlKey, string> GetSqls = new();
    internal static readonly ConcurrentDictionary<EntitySqlKey, string> DeleteSqls = new();
    internal static readonly ConcurrentDictionary<InsertColumnsKey, string> InsertColumnsSqls = new();
    internal static readonly ConcurrentDictionary<InsertValuesKey, string> InsertValuesSqls = new();
    internal static readonly ConcurrentDictionary<(string column, int row), string> ParaNames = new();

    /// <summary>
    /// Inserts an entity into table asynchronously. <br/>
    /// Returns identity only if entity has an auto-increment key and includeAutoKey is <see langword="false"/>, otherwise returns <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="con"></param>
    /// <param name="schema"></param>
    /// <param name="entity"></param>
    /// <param name="returnId"></param>
    /// <param name="includeAutoKey"></param>
    /// <param name="commandInfo"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<dynamic?> InsertAsync<T>(this DbConnection con, T entity, string? schema = null, bool returnId = true, bool includeAutoKey = false, CommandInfo commandInfo = default)
        where T : class
    {
        var value = await con.ExecuteAsync(commandInfo, m => GetInsertSql(m, schema, entity, returnId, includeAutoKey), async (a, m) =>
        {
            if (includeAutoKey && EntityDefinition<T>.Definition.HasAutoKey())
            {
                await using var x = await a.EnableIdentityInsertAsync<T>(schema, m);
                return await m.ExecuteScalarAsync();
            }
            else
            {
                return await m.ExecuteScalarAsync();
            }
        });
        // we cast value to dynamic first so that we can converting the value to a different type, such as long -> int or decimal -> long.
        return (dynamic?)value;
    }

    /// <summary>
    /// Inserts entities into table asynchronously and returns affected rows.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="con"></param>
    /// <param name="schema"></param>
    /// <param name="entities"></param>
    /// <param name="includeAutoKey"></param>
    /// <param name="commandInfo"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Task<int> BulkInsertAsync<T>(this DbConnection con, IReadOnlyCollection<T> entities, string? schema = null, bool includeAutoKey = false, CommandInfo commandInfo = default)
        where T : class
    {
        if (entities.IsNullOrEmpty())
            return Task.FromResult(0);

        return con.ExecuteAsync(commandInfo, m => GetBulkInsertSql(m, schema, entities, includeAutoKey), async (a, m) =>
        {
            if (includeAutoKey && EntityDefinition<T>.Definition.HasAutoKey())
            {
                await using var x = await a.EnableIdentityInsertAsync<T>(schema, m);
                return await m.ExecuteNonQueryAsync();
            }
            else
            {
                return await m.ExecuteNonQueryAsync();
            }
        });
    }

    internal static string GetParameterName(string column, int row)
    {
        return ParaNames.GetOrAdd((column, row), _ => $"@{column}_{row}");
    }

    internal static SqlInfo GetBulkInsertSql<T>(ISqlAdapter sqlAdapter, string? schema, IReadOnlyCollection<T> entities, bool includeAutoKey)
    {
        var entityType = typeof(T);
        var def = GetEntityDefinition(entityType);
        var columns = GetInsertColumnsSql(sqlAdapter, schema, entityType, includeAutoKey);
        var values = GetInsertValuesSql(entityType, entities.Count, includeAutoKey);

        var paras = new List<DbParameter>();
        foreach (var (item, i, _, _) in entities.IndexExt())
        {
            foreach (var field in def.InsertFields(includeAutoKey))
            {
                var paraName = GetParameterName(field.PropertyInfo.Name, i);
                var value = field.PropertyInfo.GetValue(item);
                var para = sqlAdapter.CreateParameter(paraName, value, field.DbType);
                paras.Add(para);
            }
        }

        using var sb = new ValueStringBuilder(1024);
        sb.Append(columns);
        sb.Append(Environment.NewLine);
        sb.Append(values);

        return new(sb.ToString(), paras);
    }

    internal static string GetInsertColumnsSql(ISqlAdapter sqlAdapter, string? schema, Type entityType, bool includeAutoKey)
    {
        return InsertColumnsSqls.GetOrAdd(new(sqlAdapter, schema, entityType, includeAutoKey), k => CreateInsertColumnsSql(k));

        static string CreateInsertColumnsSql(InsertColumnsKey key)
        {
            var (sqlAdapter, schema, entityType, includeAutoKey) = key;
            var def = GetEntityDefinition(entityType);
            var tableName = GetTableNameWithSchema(sqlAdapter, schema, def.EntityType);
            var sbColumnList = new StringBuilder(1024);
            foreach (var (field, _, _, isLast) in def.InsertFields(includeAutoKey).IndexExt())
            {
                sbColumnList.Append(sqlAdapter.GetQuotedColumnName(field.FieldName));
                if (isLast == false)
                {
                    sbColumnList.Append(", ");
                }
            }
            var sql = $"INSERT INTO {tableName} ({sbColumnList}) values";
            return sql;
        }
    }

    internal static string GetInsertValuesSql(Type entityType, int count, bool includeAutoKey)
    {
        return InsertValuesSqls.GetOrAdd(new(entityType, count, includeAutoKey), k => CreateInsertValuesSql(k));

        static string CreateInsertValuesSql(InsertValuesKey key)
        {
            var (entityType, count, includeAutoKey) = key;
            var sbParameterList = new StringBuilder(1024);
            var def = GetEntityDefinition(entityType);

            for (var i = 0; i < count; i++)
            {
                sbParameterList.Append('(');
                foreach (var (field, _, _, isLast) in def.InsertFields(includeAutoKey).IndexExt())
                {
                    var paraName = GetParameterName(field.PropertyInfo.Name, i);
                    sbParameterList.Append(paraName);

                    if (isLast == false)
                    {
                        sbParameterList.Append(", ");
                    }
                }
                sbParameterList.Append(')');
                if (i < count - 1)
                {
                    sbParameterList.Append(",\n");
                }
            }

            return sbParameterList.ToString();
        }
    }

    internal static SqlInfo GetInsertSql<T>(ISqlAdapter sqlAdapter, string? schema, T entity, bool returnId, bool includeAutoKey)
    {
        var (sql, paras) = GetBulkInsertSql(sqlAdapter, schema, new[] { entity }, includeAutoKey);

        // when auto key is inserted, id cannot be returned.
        // ReSharper disable once InvertIf
        if (includeAutoKey == false && returnId)
        {
            var def = GetEntityDefinition(typeof(T));
            // ReSharper disable once InvertIf
            if (def.AutoKeys.Count == 1)
            {
                // only return id when entity has single auto key.
                using var sb = new ValueStringBuilder(1024);
                sb.Append(sql);
                sb.Append(";");
                sb.Append(Environment.NewLine);
                sb.Append(sqlAdapter.SelectIdentitySql);
                return new(sb.ToString(), paras);
            }
        }

        return new(sql, paras);
    }

    /// <summary>
    /// Get an entity by id
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connection"></param>
    /// <param name="schema"></param>
    /// <param name="id"></param>
    /// <param name="commandInfo"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Task<T?> GetAsync<T>(this DbConnection connection, object id, string? schema = null, CommandInfo commandInfo = default)
    {
        var adapter = commandInfo.SqlAdapter ?? GetSqlAdapter(connection);
        var sql = GetSqls.GetOrAdd(new(adapter, schema, typeof(T)), k => CreateGetSql(k));
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        return connection.QueryFirstOrDefaultAsync<T?>(sql, dynParams, commandInfo.Transaction, commandInfo.TimeoutSeconds);

        static string CreateGetSql(EntitySqlKey key)
        {
            var (sqlAdapter, schema, entityType) = key;
            var keyField = GetSingleKey(entityType);
            var name = GetTableNameWithSchema(sqlAdapter, schema, entityType);
            var keyName = sqlAdapter.GetQuotedColumnName(keyField.FieldName);
            return $"SELECT * FROM {name} WHERE {keyName} = @id";
        }
    }

    /// <summary>
    /// Delete an entity by id
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="con"></param>
    /// <param name="schema"></param>
    /// <param name="id"></param>
    /// <param name="commandInfo"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Task<int> DeleteAsync<T>(this DbConnection con, object id, string? schema = null, CommandInfo commandInfo = default)
    {
        var adapter = commandInfo.SqlAdapter ?? GetSqlAdapter(con);
        var sql = DeleteSqls.GetOrAdd(new(adapter, schema, typeof(T)), k => CreateDeleteSql(k));
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        return con.ExecuteAsync(sql, dynParams, commandInfo.Transaction, commandInfo.TimeoutSeconds);

        static string CreateDeleteSql(EntitySqlKey key)
        {
            var (sqlAdapter, schema, entityType) = key;
            var keyField = GetSingleKey(entityType);
            var name = GetTableNameWithSchema(sqlAdapter, schema, entityType);
            var keyName = sqlAdapter.GetQuotedColumnName(keyField.FieldName);
            return $"DELETE FROM {name} WHERE {keyName} = @id";
        }
    }

    private static FieldDefinition GetSingleKey(Type type)
    {
        var def = GetEntityDefinition(type);
        var keys = def.Keys;
        if (keys.Count > 1)
            throw new DataException($"Only supports an entity with a single [Key] property. [Key] Count: {keys.Count}");
        if (keys.Count == 0)
            throw new DataException("Only supports an entity with a [Key] property");
        return keys[0];
    }

    internal static Task<T> ExecuteAsync<T>(this DbConnection con, CommandInfo commandInfo, Func<ISqlAdapter, SqlInfo> sqlFunc, Func<DbCommand, Task<T>> func)
    {
        return con.ExecuteAsync(commandInfo, sqlFunc, (_, m) => func(m));
    }

    internal static async Task<T> ExecuteAsync<T>(this DbConnection con, CommandInfo commandInfo, Func<ISqlAdapter, SqlInfo> sqlFunc, Func<ISqlAdapter, DbCommand, Task<T>> func)
    {
        var adapter = commandInfo.SqlAdapter ?? GetSqlAdapter(con);
        var (sql, paras) = sqlFunc(adapter);
        var cmd = con.CreateCommand(sql, paras, commandInfo.TimeoutSeconds, commandInfo.Transaction);
        await con.TryOpenAsync();
        return await func(adapter, cmd);
    }

    public static DbCommand CreateCommand(this DbConnection con, string sql, IEnumerable<DbParameter>? paras = null, int? timeoutSeconds = null, DbTransaction? transaction = null)
    {
        var command = con.CreateCommand();
        command.CommandText = sql;
        command.Transaction = transaction;
        foreach (var item in paras.Touch())
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