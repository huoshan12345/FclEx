using System.Collections.Concurrent;

namespace FclEx.Databases;

public record ConnectionStrings(DatabasesConfig Config, string Database, DatabaseUser User)
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
            _ => throw new NotSupportedException($"Unsupported database driver type: {dbDriver}")
        };
    }

    private ConnectionStringBuilder Create(DbDriver dbDriver, bool isUser, string? database)
    {
        var config = Get(dbDriver);
        var (username, password) = isUser
            ? (User.UserName, User.Password)
            : (config.UserName, config.Password);
        var builder = new ConnectionStringBuilder(
            DbDriver: dbDriver,
            Host: config.Host,
            Port: config.Port,
            UserName: username,
            Password: password,
            Database: database ?? Database);
        return builder;
    }

    private static readonly ConcurrentDictionary<(DbDriver, bool, string?), ConnectionStringBuilder> _cache = new();

    public ConnectionStringBuilder Get(DbDriver dbDriver, bool isUser, string? database = null)
    {
        var key = (dbDriver, isUser, database);
        var builder = _cache.GetOrAdd(key, k => Create(k.Item1, k.Item2, k.Item3));
        return builder;
    }

    public ConnectionStringBuilder Get(DbDriver dbDriver, string? database, bool isUser = false)
    {
        return Get(dbDriver, isUser, database);
    }
}