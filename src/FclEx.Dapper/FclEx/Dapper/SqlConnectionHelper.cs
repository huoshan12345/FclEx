namespace FclEx.Dapper;

public static class SqlConnectionHelper
{
    public static DnsEndPoint ParseEndpoint(string dataSource)
    {
        Check.NotEmpty(dataSource);

        var (host, portStr) = dataSource.Partition(",");
        var port = int.TryParse(portStr, out var p) ? p : 1433;
        return new(host, port);
    }
}