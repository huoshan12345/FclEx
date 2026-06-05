namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Adapter for MySqlConnector
/// </summary>
public class MySqlConnectorAdapter : SqlAdapterBase<MySqlConnectorAdapter>
{
    public override string SelectIdentitySql { get; } = "SELECT LAST_INSERT_ID()";

    protected override QuotationMarks QuotationMarks { get; } = new('`');

    protected override DbParameterCreator BuildParameterCreator()
    {
        return BuildParameterCreator("MySqlConnector.MySqlParameter, MySqlConnector", "MySqlDbType");
    }
}