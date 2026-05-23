namespace FclEx.Databases;

public class DatabasesConfig
{
    public DatabaseConfig SqlServer { get; set; } = default!;
    public DatabaseConfig MySql { get; set; } = default!;
    public DatabaseConfig Postgres { get; set; } = default!;
    public DatabaseConfig Sqlite { get; set; } = default!;
}