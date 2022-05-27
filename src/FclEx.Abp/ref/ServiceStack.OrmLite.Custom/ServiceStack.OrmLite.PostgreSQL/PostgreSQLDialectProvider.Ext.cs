using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using FclEx;
using FclEx.Extensions;
using Npgsql;

namespace ServiceStack.OrmLite.PostgreSQL
{
    public partial class PostgreSqlDialectProvider
    {
        public override bool IfDatabaseExists(string connectionString)
        {
            try
            {
                // When checking whether a database exists, pooling must be off, otherwise we may
                // attempt to reuse a pooled connection, which may be broken (this happened in the tests).
                var unpooledCsb = new NpgsqlConnectionStringBuilder(connectionString) { Pooling = false };
                using (var unpooledConn = CreateConnection(unpooledCsb.ConnectionString, null))
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    unpooledConn.Open();
                    unpooledConn.Close();
                }
                return true;
            }
            catch (PostgresException e) when (e.SqlState == "3D000")
            {
                return false;
            }
            catch (NpgsqlException e) when (
                e.InnerException is IOException
                && e.InnerException.InnerException is SocketException exception
                && exception.SocketErrorCode == SocketError.ConnectionReset
            )
            {
                // Pretty awful hack around #104
                return false;
            }
        }

        public override void CreateDatabase(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString) { Pooling = false };
            var database = builder.Database;
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(builder.Database));

            builder.Database = "postgres";
            try
            {
                using (var conn = CreateConnection(builder.ConnectionString, null))
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = $"CREATE DATABASE \"{database}\";";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (PostgresException e) when (e.SqlState == "23505"
                                              && e.ConstraintName == "pg_database_datname_index"
            )
            {
                // This occurs when two connections are trying to create the same database concurrently
                // (happens in the tests). Simply ignore the error.
            }

        }

        public override async Task<bool> IfDatabaseExistsAsync(string connectionString)
        {
            try
            {
                // When checking whether a database exists, pooling must be off, otherwise we may
                // attempt to reuse a pooled connection, which may be broken (this happened in the tests).
                var unpooledCsb = new NpgsqlConnectionStringBuilder(connectionString) { Pooling = false };
                using (var unpooledConn = CreateConnection(unpooledCsb.ConnectionString, null))
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    await OpenAsync(unpooledConn).DonotCapture();
                }
                return true;
            }
            catch (PostgresException e) when (e.SqlState == "3D000")
            {
                return false;
            }
            catch (NpgsqlException e) when (
                e.InnerException is IOException
                && e.InnerException.InnerException is SocketException exception
                && exception.SocketErrorCode == SocketError.ConnectionReset
            )
            {
                // Pretty awful hack around #104
                return false;
            }
        }

        public override async Task CreateDatabaseAsync(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString) { Pooling = false };
            var database = builder.Database;
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(builder.Database));

            builder.Database = "postgres";
            try
            {
                using (var con = CreateConnection(builder.ConnectionString, null))
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    await OpenAsync(con).DonotCapture();
                    var cmd = con.CreateCommand();
                    cmd.CommandText = $"CREATE DATABASE \"{database}\";";
                    await cmd.ExecNonQueryAsync();
                }
            }
            catch (PostgresException e) when (e.SqlState == "23505"
                                              && e.ConstraintName == "pg_database_datname_index"
            )
            {
                // This occurs when two connections are trying to create the same database concurrently
                // (happens in the tests). Simply ignore the error.
            }
        }
    }
}
