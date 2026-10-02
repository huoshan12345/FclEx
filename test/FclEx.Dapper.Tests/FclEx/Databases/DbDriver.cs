namespace FclEx.Databases;

public enum DbDriver
{
    SqlServer,
    Sqlite,
    Npgsql,
    MySql,
    MySqlConnector,
    Oracle,
}

public static class DbDriverExtensions
{
    public static bool IsMySql(this DbDriver dbDriver)
        => dbDriver is DbDriver.MySql or DbDriver.MySqlConnector;
}