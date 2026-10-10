using System.Data.Common;

namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Provides PostgreSQL quoting, parameter construction, batch limits, and generated-key retrieval for Npgsql.
/// </summary>
public class NpgsqlAdapter : SqlAdapterBase
{
    /// <inheritdoc />
    public override async Task<bool> ReseedIdentityAsync(DbCommand command, string tableName, string columnName,
        string? schema, CancellationToken cancellationToken = default)
    {
        var table = PrepareIdentityCommand(command, tableName, columnName, schema);
        command.Parameters.Clear();
        command.Parameters.Add(CreateParameter("table", table));
        command.Parameters.Add(CreateParameter("column", columnName));
        var sequence = await ReadIdentityScalarAsync(command,
            "SELECT pg_get_serial_sequence(@table, @column) FROM pg_attribute " +
            "WHERE attrelid = CAST(@table AS regclass) AND attname = @column AND attnum > 0 AND NOT attisdropped", cancellationToken);
        if (sequence is null or DBNull)
            return false;
        command.Parameters.Clear();
        command.Parameters.Add(CreateParameter("sequence", sequence));
        var increment = Convert.ToDecimal(await ReadIdentityScalarAsync(command,
            "SELECT seqincrement FROM pg_sequence WHERE seqrelid = CAST(@sequence AS regclass)", cancellationToken));
        if (increment <= 0)
            throw new NotSupportedException("Identity synchronization requires an ascending PostgreSQL sequence.");
        var start = Convert.ToDecimal(await ReadIdentityScalarAsync(command,
            "SELECT seqstart FROM pg_sequence WHERE seqrelid = CAST(@sequence AS regclass)", cancellationToken));
        var maximum = await ReadIdentityScalarAsync(command,
            $"SELECT MAX({GetQuotedColumnName(columnName)}) FROM {table}", cancellationToken);
        var next = maximum is null or DBNull ? start : Math.Max(start, checked(Convert.ToDecimal(maximum) + increment));
        command.Parameters.Add(CreateParameter("next", checked((long)next)));
        await ReadIdentityScalarAsync(command, "SELECT setval(CAST(@sequence AS regclass), @next, false)", cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public override string BuildTruncateCommandText(string quotedTableName, bool? restartIdentity, bool cascade)
    {
        var sql = $"TRUNCATE TABLE {quotedTableName}";
        if (restartIdentity is { } restart)
            sql += restart ? " RESTART IDENTITY" : " CONTINUE IDENTITY";
        if (cascade)
            sql += " CASCADE";
        return sql + ";";
    }

    /// <inheritdoc />
    /// <remarks>Resolves unqualified names through search_path and includes regular and partitioned tables.</remarks>
    public override string BuildTableExistsCommandText(string tableNameParameter, string? schemaParameter = null)
    {
        var table = $"pg_catalog.quote_ident({tableNameParameter}::text)";
        if (schemaParameter is not null)
            table = $"CASE WHEN {schemaParameter}::text IS NULL THEN {table} ELSE pg_catalog.quote_ident({schemaParameter}::text) || '.' || {table} END";
        return $"SELECT COUNT(*) FROM pg_catalog.pg_class WHERE oid = pg_catalog.to_regclass({table}) AND relkind IN ('r', 'p')";
    }

    private const int MaxParametersPerCommand = 65535;

    /// <inheritdoc />
    protected override QuotationMarks QuotationMarks { get; } = new('"');

    /// <inheritdoc />
    /// <remarks>
    /// Converts <see cref="ushort"/> to <see cref="int"/>, <see cref="uint"/> to <see cref="long"/>, and
    /// <see cref="ulong"/> to <see cref="decimal"/> without losing precision, allowing Npgsql to infer
    /// PostgreSQL integer, bigint, and numeric parameters. Byte values remain unchanged.
    /// Explicit oid, xid, xid8, cid, regtype, and regconfig parameters retain their original
    /// values because these PostgreSQL types use unsigned representations. Null becomes <see cref="DBNull.Value"/>.
    /// DateTimeOffset values are normalized to UTC when no store type is supplied or when the explicit type is
    /// TimestampTz, timestamptz, or timestamp with time zone. This preserves the instant, not the original offset.
    /// Timestamp and timestamp without time zone leave values unchanged; the adapter does not choose a wall-clock
    /// interpretation for them. This decision uses parameter mapping, not database column metadata.
    /// An explicit store type still determines the provider parameter type and may reject an incompatible value.
    /// </remarks>
    public override DbParameter CreateParameter(string name, object? value, string? storeTypeName = null)
    {
        storeTypeName = storeTypeName?.Trim().ToLowerInvariant() switch
        {
            "timestamptz" or "timestamp with time zone" => "TimestampTz",
            "timestamp" or "timestamp without time zone" => "Timestamp",
            _ => storeTypeName,
        };

        if (value is DateTimeOffset timestamp && (storeTypeName is null || storeTypeName == "TimestampTz"))
            value = timestamp.ToUniversalTime();

        if (storeTypeName?.Trim().ToLowerInvariant() is not
            ("oid" or "xid" or "xid8" or "cid" or "regtype" or "regconfig"))
        {
            value = value switch
            {
                ushort unsignedValue => (int)unsignedValue,
                uint unsignedValue => (long)unsignedValue,
                ulong unsignedValue => (decimal)unsignedValue,
                _ => value,
            };
        }

        return base.CreateParameter(name, value, storeTypeName);
    }

    /// <inheritdoc />
    /// <remarks>Applies PostgreSQL's 65,535-parameter command limit.</remarks>
    public override int GetMaxInsertBatchSize(int parameterCountPerRow)
    {
        return CalculateMaxInsertBatchSize(parameterCountPerRow, MaxParametersPerCommand);
    }

    /// <inheritdoc />
    /// <remarks>Appends <c>RETURNING</c> when a generated-key column is requested.</remarks>
    public override string BuildInsertCommandText(
        string quotedTableName,
        string? columnListSql,
        string? valueRowsSql,
        string? quotedGeneratedKeyColumn)
    {
        var sql = base.BuildInsertCommandText(quotedTableName, columnListSql, valueRowsSql, null);
        return quotedGeneratedKeyColumn is null
            ? sql
            : $"{sql}{Environment.NewLine}RETURNING {quotedGeneratedKeyColumn}";
    }
    
    /// <inheritdoc />
    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("Npgsql.NpgsqlParameter, Npgsql", "NpgsqlDbType");
    }
}
