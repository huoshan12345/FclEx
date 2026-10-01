using System.Data.Common;

namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Provides PostgreSQL quoting, parameter construction, batch limits, and generated-key retrieval for Npgsql.
/// </summary>
public class NpgsqlAdapter : SqlAdapterBase
{
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
    /// An explicit store type still determines the provider parameter type and may reject an incompatible value.
    /// </remarks>
    public override DbParameter CreateParameter(string name, object? value, string? storeTypeName = null)
    {
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
