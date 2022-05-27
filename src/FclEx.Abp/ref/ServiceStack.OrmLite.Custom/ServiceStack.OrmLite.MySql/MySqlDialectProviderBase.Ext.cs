using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using FclEx;
using FclEx.Extensions;
using MySql.Data.MySqlClient;

namespace ServiceStack.OrmLite.MySql
{
    public partial class MySqlDialectProviderBase<TDialect>
    {
        public override void CreateDatabase(string connectionString)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString) { Pooling = false };
            var database = builder.Database;
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(builder.Database));
            builder.Database = "mysql";
            using (var con = CreateConnection(builder.ConnectionString, null))
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = $"CREATE DATABASE {database};";
                cmd.ExecuteNonQuery();
            }
        }

        public override bool IfDatabaseExists(string connectionString)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString) { Pooling = false };
            var database = builder.Database;
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(builder.Database));
            builder.Database = string.Empty;
            using (var con = CreateConnection(builder.ConnectionString, null))
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = $"SHOW DATABASES LIKE '{database}'";
                return cmd.ExecuteScalar() is string result
                       && result.Equals(database, StringComparison.OrdinalIgnoreCase);
            }
        }

        public override async Task CreateDatabaseAsync(string connectionString)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString) { Pooling = false };
            var database = builder.Database;
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(builder.Database));
            builder.Database = "mysql";
            using (var con = CreateConnection(builder.ConnectionString, null))
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                await OpenAsync(con).DonotCapture();
                var cmd = con.CreateCommand();
                cmd.CommandText = $"CREATE DATABASE {database};";
                await cmd.ExecNonQueryAsync();
            }
        }

        public override async Task<bool> IfDatabaseExistsAsync(string connectionString)
        {
            var builder = new MySqlConnectionStringBuilder(connectionString) { Pooling = false };
            var database = builder.Database;
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(builder.Database));
            builder.Database = string.Empty;
            using (var con = CreateConnection(builder.ConnectionString, null))
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                await OpenAsync(con).DonotCapture();
                var cmd = con.CreateCommand();
                cmd.CommandText = $"SHOW DATABASES LIKE '{database}'";
                var obj = await cmd.ScalarAsync();
                return obj is string result
                       && result.Equals(database, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
