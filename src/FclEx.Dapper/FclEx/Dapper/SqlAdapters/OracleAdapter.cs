namespace FclEx.Dapper.SqlAdapters;

public class OracleAdapter : SqlAdapterBase
{
    /// <inheritdoc />
    public override async Task<bool> SynchronizeIdentitySequenceAsync(DbCommand command, string tableName, string columnName,
        string? schema, CancellationToken cancellationToken = default)
    {
        var table = PrepareIdentityCommand(command, tableName, columnName, schema);
        var generation = await ReadIdentityScalarAsync(command,
            "SELECT generation_type FROM all_tab_identity_cols WHERE table_name = TO_CHAR(:tableName) " +
            "AND column_name = TO_CHAR(:columnName) AND owner = COALESCE(TO_CHAR(:schema), SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA'))",
            cancellationToken);
        if (generation is null or DBNull)
            return false;
        // Generation type comes from Oracle's catalog, but validate before composing DDL.
        var mode = Convert.ToString(generation)?.Trim();
        if (mode is not ("ALWAYS" or "BY DEFAULT" or "BY DEFAULT ON NULL"))
            throw new NotSupportedException($"Unsupported Oracle identity generation mode '{mode}'.");
        command.Parameters.Clear();
        command.CommandText = $"ALTER TABLE {table} MODIFY {GetQuotedColumnName(columnName)} GENERATED {mode} AS IDENTITY (START WITH LIMIT VALUE)";
        await command.ExecuteNonQueryAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public override string BuildTruncateCommandText(string quotedTableName, bool? restartIdentity, bool cascade)
    {
        if (restartIdentity == true)
            throw new NotSupportedException("Oracle TRUNCATE does not restart identity sequences.");
        return $"TRUNCATE TABLE {quotedTableName}" + (cascade ? " CASCADE" : "");
    }

    /// <inheritdoc />
    /// <remarks>Uses the session's CURRENT_SCHEMA when no owner is supplied. Names retain their exact catalog casing.</remarks>
    public override string BuildTableExistsCommandText(string tableNameParameter, string? schemaParameter = null)
    {
        var owner = "SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA')";
        if (schemaParameter is not null)
            owner = $"COALESCE(TO_CHAR({schemaParameter}), {owner})";
        // Native catalog names and SYS_CONTEXT use the database character set, while string
        // parameters default to NVarchar2. Normalize before composing or comparing catalog values.
        return $"SELECT COUNT(*) FROM all_tables WHERE table_name = TO_CHAR({tableNameParameter}) AND owner = {owner}";
    }

    private const int MaxParametersPerCommand = 65535;
    private const string GeneratedKeyParameterName = "fclex_generated_key";

    private static readonly Regex MultipleValueRows = new(@"\)\s*,\s*\(", RegexOptions.Compiled);
    private static readonly Regex StoreTypeFacets = new(@"\([^)]*\)", RegexOptions.Compiled);

    protected override QuotationMarks QuotationMarks { get; } = new('"');

    public override string GetParameterPlaceholder(string name)
    {
        return $":{name}";
    }

    public override int GetMaxInsertBatchSize(int parameterCountPerRow)
    {
        return CalculateMaxInsertBatchSize(parameterCountPerRow, MaxParametersPerCommand);
    }

    public override string BuildInsertCommandText(
        string quotedTableName,
        string? columnListSql,
        string? valueRowsSql,
        string? quotedGeneratedKeyColumn)
    {
        if ((columnListSql is null) != (valueRowsSql is null))
            throw new ArgumentException("The column list and value rows must either both be supplied or both be null.");

        if (columnListSql is null)
        {
            columnListSql = quotedGeneratedKeyColumn;
            valueRowsSql = "(DEFAULT)";
        }

        var sql = base.BuildInsertCommandText(quotedTableName, columnListSql, valueRowsSql, null);
        if (quotedGeneratedKeyColumn is null)
            return sql;

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (MultipleValueRows.IsMatch(valueRowsSql!))
            throw new NotSupportedException("Oracle generated-key retrieval supports only a single inserted row.");

        return $"{sql}{Environment.NewLine}RETURNING {quotedGeneratedKeyColumn} INTO {GetParameterPlaceholder(GeneratedKeyParameterName)}";
    }

    public override async Task<object?> ExecuteInsertReturningAsync(
        DbCommand command,
        Type generatedKeyType,
        CancellationToken cancellationToken = default)
    {
        var keyType = Nullable.GetUnderlyingType(generatedKeyType) ?? generatedKeyType;
        if (keyType.IsEnum)
            keyType = Enum.GetUnderlyingType(keyType);

        var parameter = command.CreateParameter();
        parameter.ParameterName = GeneratedKeyParameterName;
        parameter.Direction = ParameterDirection.Output;
        parameter.DbType = Type.GetTypeCode(keyType) switch
        {
            TypeCode.Byte or TypeCode.SByte or TypeCode.Int16 or TypeCode.UInt16 or
                TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or
                TypeCode.Decimal => DbType.Decimal,
            TypeCode.Single => DbType.Single,
            TypeCode.Double => DbType.Double,
            TypeCode.String or TypeCode.Char => DbType.String,
            TypeCode.DateTime => DbType.DateTime,
            _ => throw new NotSupportedException($"Oracle generated-key retrieval does not support CLR type '{generatedKeyType.FullName}'."),
        };
        if (parameter.DbType == DbType.String)
            parameter.Size = 4000;

        command.Parameters.Add(parameter);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return parameter.Value;
    }

    public override DbParameter CreateParameter(string name, object? value, string? storeTypeName = null)
    {
        value = value switch
        {
            Guid guid => guid.ToByteArray(),
            ushort unsignedValue => (int)unsignedValue,
            uint unsignedValue => (long)unsignedValue,
            ulong unsignedValue => (decimal)unsignedValue,
            _ => value,
        };

        var typeName = storeTypeName?.Trim();
        if (typeName is not null)
            typeName = StoreTypeFacets.Replace(typeName, string.Empty).Trim();

        typeName = typeName?.ToUpperInvariant() switch
        {
            "NUMBER" or "NUMERIC" or "DECIMAL" or "INTEGER" or "INT" or "SMALLINT" or
                "FLOAT" or "DOUBLE PRECISION" or "REAL" => "Decimal",
            "BINARY_DOUBLE" => "Double",
            "BINARY_FLOAT" => "Single",
            "TIMESTAMP WITH TIME ZONE" => "TimeStampTZ",
            "TIMESTAMP WITH LOCAL TIME ZONE" => "TimeStampLTZ",
            "INTERVAL DAY TO SECOND" => "IntervalDS",
            "INTERVAL YEAR TO MONTH" => "IntervalYM",
            _ => typeName,
        };

        if (value is string && string.IsNullOrEmpty(typeName))
            typeName = "NVarchar2";

        return base.CreateParameter(name.TrimStart('@', ':'), value, typeName);
    }

    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("Oracle.ManagedDataAccess.Client.OracleParameter, Oracle.ManagedDataAccess", "OracleDbType");
    }
}
