using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Adapter for Microsoft.Data.Sqlite
/// </summary>
public class SqliteAdapter : AbstractSqlAdapter<SqliteAdapter>
{
    public override bool SupportSchema { get; } = false;
    public override string SelectIdentitySql { get; } = "SELECT last_insert_rowid()";

    protected override QuotationMarks QuotationMarks { get; } = new('"');

    protected override DbParameterCreater BuildParameterCreater()
    {
        return BuildParameterCreater("Microsoft.Data.Sqlite.SqliteParameter, Microsoft.Data.Sqlite", "SqliteType");
    }
}