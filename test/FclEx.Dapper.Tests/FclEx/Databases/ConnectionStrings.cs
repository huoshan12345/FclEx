namespace FclEx.Databases;

public record ConnectionStrings(DatabasesConfig Config, DatabaseUser User)
{
    private DatabaseConfig Get(DbDriver dbDriver)
    {
        return dbDriver switch
        {
            DbDriver.SqlServer => Config.SqlServer,
            DbDriver.Sqlite => Config.Sqlite,
            DbDriver.Npgsql => Config.Postgres,
            DbDriver.MySql => Config.MySql,
            DbDriver.MySqlConnector => Config.MySql,
#if SUPPORT_ORACLE
            DbDriver.Oracle => Config.Oracle,
#endif
            _ => throw new NotSupportedException($"Unsupported database driver type: {dbDriver}")
        };
    }

    private ConnectionStringBuilder Create(DbDriver dbDriver, bool isUser, string database)
    {
        var config = Get(dbDriver);
        var (userName, password) = isUser
            ? (User.UserName, User.Password)
            : (config.UserName, config.Password);

        if (dbDriver is DbDriver.SqlServer && isUser && userName == DefaultUserName)
        {
            password = SqlServerUserPassword;
        }

        if (dbDriver is DbDriver.Oracle && isUser)
        {
            database = userName;
        }

        var builder = new ConnectionStringBuilder(
            DbDriver: dbDriver,
            Config: config with { UserName = userName, Password = password },
            Database: database);
        return builder;
    }

    private static readonly ConcurrentDictionary<(DbDriver, bool, string), ConnectionStringBuilder> _cache = new();

    public ConnectionStringBuilder Get(DbDriver dbDriver, string database, bool isUser = false)
    {
        var key = (dbDriver, isUser, database);
        var builder = _cache.GetOrAdd(key, k => Create(k.Item1, k.Item2, k.Item3));
        return builder;
    }
}