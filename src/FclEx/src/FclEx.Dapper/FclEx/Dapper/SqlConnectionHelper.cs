namespace FclEx.Dapper;

public static class SqlConnectionHelper
{
    public static SocketEndpoint ParseEndpoint(string dataSource)
    {
        Check.NotEmpty(dataSource);

        var (host, portStr) = dataSource.Cleave(",");
        var port = int.TryParse(portStr, out var p) ? p : 1433;
        return (host, port);
    }
}