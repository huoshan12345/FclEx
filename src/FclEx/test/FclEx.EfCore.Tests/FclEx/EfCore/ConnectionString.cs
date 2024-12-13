using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace FclEx.EfCore;

public class ConnectionStrings(string database, DatabaseUser user)
{
    public readonly ConnectionString Postgresql = new(DbProviderType.Npgsql, $"Server=localhost;Database={database};Port=5432;User Id=postgres;Password=111111", user);
    // NOTE: do not include 'Integrated Security=sspi;' into the sql server connection string otherwise the default schema won't work.
    public readonly ConnectionString SqlServer = new(DbProviderType.SqlServer, $@"Data Source=localhost\sqlexpress;Database={database};User Id=sa;Password=a.o7a@bj;Encrypt=false", user);
    public readonly ConnectionString MySql = new(DbProviderType.MySql, $"Server=localhost;Database={database};Port=3306;User Id=root;Password=111111;SslMode=Required", user);
    public readonly ConnectionString Sqlite = new(DbProviderType.Sqlite, $"Data Source=./{database}.sqlite;", user);

    public ConnectionString Get(DbProviderType dbProviderType)
    {
        return dbProviderType switch
        {
            DbProviderType.MySql => MySql,
            DbProviderType.MySqlConnector => MySql,
            DbProviderType.SqlServer => SqlServer,
            DbProviderType.Npgsql => Postgresql,
            DbProviderType.Sqlite => Sqlite,
            _ => throw new ArgumentOutOfRangeException(nameof(dbProviderType), dbProviderType, null)
        };
    }

    public string Get(DbProviderType dbProviderType, bool isUser)
    {
        var strings = Get(dbProviderType);
        return isUser ? strings.User : strings.Primary;
    }
}

public readonly record struct ConnectionString(DbProviderType DbProviderType, string Primary, string User)
{
    public ConnectionString(DbProviderType dbProviderType, string primary, DatabaseUser user)
        : this(dbProviderType, primary, CreateUserConnectionString(dbProviderType, primary, user))
    {
    }

    private static string CreateUserConnectionString(DbProviderType dbProviderType, string primary, DatabaseUser user)
    {
        return dbProviderType switch
        {
            DbProviderType.Npgsql => new NpgsqlConnectionStringBuilder(primary) { Username = user.Username, Password = user.Password }.ConnectionString,
            DbProviderType.SqlServer => new SqlConnectionStringBuilder(primary) { UserID = user.Username, Password = user.Password }.ConnectionString,
            DbProviderType.Sqlite => primary,
            DbProviderType.MySql => CreateUserConnectionStringForMysql(),
            DbProviderType.MySqlConnector => CreateUserConnectionStringForMysql(),
            _ => throw new ArgumentOutOfRangeException(nameof(dbProviderType), dbProviderType, null)
        };

        string CreateUserConnectionStringForMysql()
        {
            return new MySqlConnectionStringBuilder(primary)
            {
                UserID = user.Username,
                Password = user.Password,
                Database = user.DefaultSchema,
            }.ConnectionString;
        }
    }
}