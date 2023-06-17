namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Adapter for MySqlConnector
/// </summary>
public class MySqlConnectorAdapter : AbstractSqlAdapter<MySqlConnectorAdapter>
{
    public override string SelectIdentitySql { get; } = "SELECT LAST_INSERT_ID()";

    protected override QuotationMarks QuotationMarks { get; } = new('`');

    protected override DbParameterCreater BuildParameterCreater()
    {
        return BuildParameterCreater("MySqlConnector.MySqlParameter, MySqlConnector", "MySqlDbType");
    }
}