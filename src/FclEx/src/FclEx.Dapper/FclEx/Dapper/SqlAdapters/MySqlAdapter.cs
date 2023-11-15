namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Adapter for MySql.Data.MySqlClient
/// </summary>
public class MySqlAdapter : AbstractSqlAdapter<MySqlAdapter>
{
    public override bool SupportSchema { get; } = false;
    public override string SelectIdentitySql { get; } = "SELECT LAST_INSERT_ID()";

    protected override QuotationMarks QuotationMarks { get; } = new('`');

    protected override DbParameterCreater BuildParameterCreator()
    {
        return BuildParameterCreater("MySql.Data.MySqlClient.MySqlParameter, MySql.Data", "MySqlDbType");
    }
}