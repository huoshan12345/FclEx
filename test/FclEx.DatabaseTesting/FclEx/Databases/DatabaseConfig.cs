namespace FclEx.Databases;

public record DatabaseConfig
{
    public string Host { get; init; } = "";
    public int Port { get; init; }
    public string UserName { get; init; } = "";
    public string Password { get; init; } = "";
    public string ServiceName { get; init; } = ""; // For Oracle
}