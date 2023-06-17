using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Dapper.SqlAdapters;

public class SqliteAdapter : AbstractSqlAdapter<NpgsqlAdapter>
{
    public override string SelectIdentitySql { get; } = "SELECT last_insert_rowid()";

    protected override QuotationMarks QuotationMarks { get; } = new('"');

    public override DbParameter CreateParameter(string name, object? value, string? type = null)
    {
        throw new NotImplementedException();
    }
}