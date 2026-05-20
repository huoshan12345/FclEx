using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Npgsql;
using SQLitePCL;

namespace FclEx.Databases;

public record ConnectionStringBuilder(
    DbDriver DbDriver,
    string Host,
    int Port,
    string UserName,
    string Password,
    string Database)
{
    public ConnectionStringBuilder WithUser(DatabaseUser user)
    {
        return this with { UserName = user.UserName, Password = user.Password };
    }

    public string Build()
    {
        return DbDriver switch
        {
            DbDriver.SqlServer => new SqlConnectionStringBuilder
            {
                DataSource = Host,
                InitialCatalog = Database,
                UserID = UserName,
                Password = Password,
            }.ConnectionString,
            DbDriver.Sqlite => new SqliteConnectionStringBuilder { DataSource = $"./{Database}.sqlite" }.ConnectionString,
            DbDriver.Npgsql => new NpgsqlConnectionStringBuilder
            {
                Host = Host,
                Database = Database,
                Port = Port,
                Username = UserName,
                Password = Password,
            }.ConnectionString,
            DbDriver.MySql or DbDriver.MySqlConnector => new MySqlConnectionStringBuilder
            {
                Server = Host,
                Database = Database,
                Port = (uint)Port,
                UserID = UserName,
                Password = Password,
                SslMode = MySqlSslMode.Required,
            }.ConnectionString,
            _ => throw new ArgumentOutOfRangeException(nameof(DbDriver), DbDriver, null),
        };
    }

    public static DbConnection CreateDbConnection(DbDriver dbDriver, string connectionString)
    {
        return dbDriver switch
        {
            DbDriver.SqlServer => new SqlConnection(connectionString),
            DbDriver.Sqlite => new SqliteConnection(connectionString),
            DbDriver.Npgsql => new NpgsqlConnection(connectionString),
            DbDriver.MySql => new MySqlConnection(connectionString),
            DbDriver.MySqlConnector => new MySqlConnector.MySqlConnection(connectionString),
            _ => throw new ArgumentOutOfRangeException(nameof(dbDriver), dbDriver, null)
        };
    }

    public DbConnection CreateDbConnection()
    {
        return CreateDbConnection(DbDriver, Build());
    }
}