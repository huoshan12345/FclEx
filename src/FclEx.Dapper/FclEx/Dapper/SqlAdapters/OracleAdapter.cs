namespace FclEx.Dapper.SqlAdapters;

public class OracleAdapter : SqlAdapterBase
{
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
