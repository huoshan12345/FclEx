namespace FclEx.EfCore;

public class ConnectionStrings(string database, DatabaseUser user)
{
#if !DISABLE_NPGSQL
    public static DatabaseConfig Postgres => EfCoreFixture.Postgres;
    public readonly ConnectionString Postgresql = new(DbProviderType.Npgsql, $"Server={Postgres.Host};Database={database};Port={Postgres.Port};User Id={Postgres.UserName};Password={Postgres.Password}", user);
#endif
#if !DISABLE_MYSQL    
    public readonly ConnectionString MySql = new(DbProviderType.MySql, $"Server=127.0.0.1;Database={database};Port=3306;User Id=root;Password=111111;SslMode=Required", user);
#endif

    // NOTE: do not include 'Integrated Security=sspi;' into the sql server connection string otherwise the default schema won't work.
    public readonly ConnectionString SqlServer = new(DbProviderType.SqlServer, $@"Data Source=(localdb)\MSSQLLocalDB;Database={database};", user);
    public readonly ConnectionString Sqlite = new(DbProviderType.Sqlite, $"Data Source=./{database}.sqlite;", user);

    public ConnectionString Get(DbProviderType dbProviderType)
    {
        return dbProviderType switch
        {
#if !DISABLE_NPGSQL
            DbProviderType.Npgsql => Postgresql,
#endif
#if !DISABLE_MYSQL
            DbProviderType.MySql => MySql,
            DbProviderType.MySqlConnector => MySql,
#endif
            DbProviderType.SqlServer => SqlServer,
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
            DbProviderType.SqlServer => new SqlConnectionStringBuilder(primary) { UserID = user.Username, Password = user.Password }.ConnectionString,
            DbProviderType.Sqlite => primary,
#if !DISABLE_NPGSQL
            DbProviderType.Npgsql => new NpgsqlConnectionStringBuilder(primary) { Username = user.Username, Password = user.Password }.ConnectionString,
#endif
#if !DISABLE_MYSQL
            DbProviderType.MySql or DbProviderType.MySqlConnector => CreateUserConnectionStringForMysql(),
#endif
            _ => throw new ArgumentOutOfRangeException(nameof(dbProviderType), dbProviderType, null)
        };

#if !DISABLE_MYSQL
        string CreateUserConnectionStringForMysql()
        {
            return new MySqlConnectionStringBuilder(primary)
            {
                UserID = user.Username,
                Password = user.Password,
                Database = user.DefaultSchema,
            }.ConnectionString;
        }
#endif
    }
}