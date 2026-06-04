namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Adapter for Npgsql
/// </summary>
public class NpgsqlAdapter : SqlAdapterBase<NpgsqlAdapter>
{
    public override string SelectIdentitySql { get; } = "SELECT LASTVAL()";

    protected override QuotationMarks QuotationMarks { get; } = new('"');
    
    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("Npgsql.NpgsqlParameter, Npgsql", "NpgsqlDbType");
    }
}