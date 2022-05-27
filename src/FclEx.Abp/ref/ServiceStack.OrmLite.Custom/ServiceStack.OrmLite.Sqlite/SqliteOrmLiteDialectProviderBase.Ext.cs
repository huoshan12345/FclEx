using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FclEx;
using FclEx.Extensions;

namespace ServiceStack.OrmLite.Sqlite
{
    public partial class SqliteOrmLiteDialectProviderBase
    {
        public override bool IfDatabaseExists(string connectionString)
        {
            var conStr = new SQLiteConnectionStringBuilder(connectionString);
            return conStr.DataSource != ":memory:" && File.Exists(conStr.DataSource);
        }

        public override void CreateDatabase(string connectionString)
        {
            using var con = CreateConnection(connectionString);
            con.Open();
        }

        public override Task<bool> IfDatabaseExistsAsync(string connectionString)
        {
            return IfDatabaseExists(connectionString).ToTask();
        }

        public override async Task CreateDatabaseAsync(string connectionString)
        {
            using var con = CreateConnection(connectionString);
            await OpenAsync(con).DonotCapture();
        }
    }
}
