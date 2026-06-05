namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Adapter for MySql.Data.MySqlClient
/// </summary>
public class MySqlAdapter : SqlAdapterBase<MySqlAdapter>
{
    public override bool SupportSchema { get; } = false;
    public override string SelectIdentitySql { get; } = "SELECT LAST_INSERT_ID()";

    protected override QuotationMarks QuotationMarks { get; } = new('`');

    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("MySql.Data.MySqlClient.MySqlParameter, MySql.Data", "MySqlDbType");
    }
}