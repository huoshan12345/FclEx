using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
#if SUPPORT_ORACLE
using Oracle.ManagedDataAccess.Client;
#endif

namespace FclEx.Databases;

public enum TestLogin
{
    Standard,
    DefaultSchemaUser,
}

/// <summary>A resolved physical target. Oracle's login user and object owner are not a database name.</summary>
public sealed record TestDatabaseTarget(
    DbDriver Driver,
    DatabaseConfig Login,
    string? Database = null,
    string? Schema = null,
    string? FilePath = null)
{
    public string BuildConnectionString() => Driver switch
    {
        DbDriver.SqlServer => new SqlConnectionStringBuilder
        {
            DataSource = Login.Host,
            InitialCatalog = Database,
            UserID = Login.UserName,
            Password = Login.Password,
            ConnectTimeout = 3,
            ConnectRetryInterval = 1,
            ConnectRetryCount = 1,
            TrustServerCertificate = true,
        }.ConnectionString,
        DbDriver.Npgsql => new NpgsqlConnectionStringBuilder
        {
            Host = Login.Host,
            Port = Login.Port,
            Database = Database,
            Username = Login.UserName,
            Password = Login.Password,
        }.ConnectionString,
        DbDriver.MySql => new MySqlConnectionStringBuilder
        {
            Server = Login.Host,
            Port = (uint)Login.Port,
            Database = Database,
            UserID = Login.UserName,
            Password = Login.Password,
            SslMode = MySqlSslMode.Required,
            MaximumPoolSize = 16,
            ConnectionTimeout = 30,
        }.ConnectionString,
        DbDriver.MySqlConnector => new MySqlConnector.MySqlConnectionStringBuilder
        {
            // EF's MySqlConnector providers require these options before an externally owned connection is opened.
            AllowUserVariables = true,
            UseAffectedRows = false,
            Server = Login.Host,
            Port = (uint)Login.Port,
            Database = Database,
            UserID = Login.UserName,
            Password = Login.Password,
            SslMode = MySqlConnector.MySqlSslMode.Required,
            MaximumPoolSize = 16,
            ConnectionTimeout = 30,
        }.ConnectionString,
        DbDriver.Sqlite => new SqliteConnectionStringBuilder
        {
            DataSource = FilePath ?? throw new InvalidOperationException("SQLite requires a resolved file path."),
            ForeignKeys = true,
            Pooling = false,
            DefaultTimeout = 30,
        }.ConnectionString,
#if SUPPORT_ORACLE
        DbDriver.Oracle => new OracleConnectionStringBuilder
        {
            DataSource = $"{Login.Host}:{Login.Port}/{Login.ServiceName}",
            UserID = Login.UserName.EnsureDoubleQuoted(),
            Password = Login.Password,
        }.ConnectionString,
#endif
        _ => throw new ArgumentOutOfRangeException(nameof(Driver), Driver, null),
    };

    public DbConnection CreateConnection()
    {
        var connectionString = BuildConnectionString();
        return Driver switch
        {
            DbDriver.SqlServer => new SqlConnection(connectionString),
            DbDriver.Sqlite => new SqliteConnection(connectionString),
            DbDriver.Npgsql => new NpgsqlConnection(connectionString),
            DbDriver.MySql => new MySqlConnection(connectionString),
            DbDriver.MySqlConnector => new MySqlConnector.MySqlConnection(connectionString),
#if SUPPORT_ORACLE
            DbDriver.Oracle => new OracleConnection(connectionString),
#endif
            _ => throw new ArgumentOutOfRangeException(nameof(Driver), Driver, null),
        };
    }
}
