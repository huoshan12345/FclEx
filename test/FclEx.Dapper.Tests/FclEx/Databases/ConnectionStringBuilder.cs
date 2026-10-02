using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;

namespace FclEx.Databases;

public record ConnectionStringBuilder(
    DbDriver DbDriver,
    DatabaseConfig Config,
    string Database)
{
    public ConnectionStringBuilder WithUser(DatabaseUser user)
    {
        return this with { Config = Config with { UserName = user.UserName, Password = user.Password } };
    }

    public string Build()
    {
        return DbDriver switch
        {
            DbDriver.SqlServer => new SqlConnectionStringBuilder
            {
                DataSource = Config.Host,
                InitialCatalog = Database,
                UserID = UserName,
                Password = Config.Password,
            }.ConnectionString,
            DbDriver.Sqlite => new SqliteConnectionStringBuilder { DataSource = $"./{Database}.sqlite" }.ConnectionString,
            DbDriver.Npgsql => new NpgsqlConnectionStringBuilder
            {
                Host = Config.Host,
                Database = Database,
                Port = Config.Port,
                Username = Config.UserName,
                Password = Config.Password,
            }.ConnectionString,
            DbDriver.MySql or DbDriver.MySqlConnector => new MySqlConnectionStringBuilder
            {
                Server = Config.Host,
                Database = Database,
                Port = (uint)Config.Port,
                UserID = Config.UserName,
                Password = Config.Password,
                SslMode = MySqlSslMode.Required,
                MaximumPoolSize = 16,
                ConnectionTimeout = 30,
            }.ConnectionString,
#if SUPPORT_ORACLE
            DbDriver.Oracle => new OracleConnectionStringBuilder
            {
                DataSource = $"{Config.Host}:{Config.Port}/{Config.ServiceName}",
                UserID = Config.UserName,
                Password = Config.Password,
            }.ConnectionString,
#endif
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
#if SUPPORT_ORACLE
            DbDriver.Oracle => new OracleConnection(connectionString),
#endif
            _ => throw new ArgumentOutOfRangeException(nameof(dbDriver), dbDriver, null)
        };
    }

    public DbConnection CreateDbConnection()
    {
        return CreateDbConnection(DbDriver, Build());
    }
}