namespace FclEx.Dapper.SqlAdapters;

public class MySqlAdapter : AbstractSqlAdapter<MySqlAdapter>
{
    public override string SelectIdentitySql { get; } = "SELECT LAST_INSERT_ID()";

    protected override QuotationMarks QuotationMarks { get; } = new('`');

    public override DbParameter CreateParameter(string name, object? value, string? type = null)
    {
        throw new NotImplementedException();
    }
}