using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using FclEx;
using FclEx.Extensions;

namespace ServiceStack.OrmLite.SqlServer
{
    partial class SqlServerOrmLiteDialectProvider
    {
        public override bool IfDatabaseExists(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString) { Pooling = false };
            var database = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(builder.InitialCatalog));
            try
            {
                using (var db = CreateConnection(builder.ConnectionString, null))
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    db.Open();
                }
                return true;
            }
            catch (SqlException e) when (IsDoesNotExist(e))
            {
                return false;
            }
        }

        private static bool IsDoesNotExist(SqlException exception)
        {
            return exception.Number == 4060 
                   || exception.Number == 1832 
                   || exception.Number == 5120;
        }

        public override void CreateDatabase(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString) { Pooling = false };
            var database = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(builder.InitialCatalog));
            builder.InitialCatalog = "master";
            builder.Remove("AttachDBFilename");
            using (var con = CreateConnection(builder.ConnectionString, null))
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = $"CREATE DATABASE {database};";
                cmd.ExecuteNonQuery();
            }
        }

        public override async Task<bool> IfDatabaseExistsAsync(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString) { Pooling = false };
            var database = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(builder.InitialCatalog));
            try
            {
                using (var con = CreateConnection(builder.ConnectionString, null))
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    await OpenAsync(con).DonotCapture();
                }
                return true;
            }
            catch (SqlException e) when (IsDoesNotExist(e))
            {
                return false;
            }
        }

        public override async Task CreateDatabaseAsync(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString) { Pooling = false };
            var database = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(builder.InitialCatalog));
            builder.InitialCatalog = "master";
            builder.Remove("AttachDBFilename");
            using (var con = CreateConnection(builder.ConnectionString, null))
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                await OpenAsync(con).DonotCapture();
                var cmd = con.CreateCommand();
                cmd.CommandText = $"CREATE DATABASE {database};";
                await cmd.ExecNonQueryAsync();
            }
        }
    }
}
