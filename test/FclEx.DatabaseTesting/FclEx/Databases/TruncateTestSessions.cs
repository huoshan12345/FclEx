namespace FclEx.Databases;

/// <summary>Opens connections and exclusively leases existing truncate-test tables without creating or dropping them.</summary>
public sealed class TruncateTestSessions(TestDatabaseEnvironment environment)
{
    private readonly ConcurrentDictionary<(DbDriver Driver, string? Schema, string Table), SemaphoreSlim> _gates = new();

    /// <summary>Waits for the requested tables, then opens a connection; disposal releases the leases but preserves tables and rows.</summary>
    /// <remarks>Tests own row cleanup. Leases are local to this fixture and do not coordinate separate processes.</remarks>
    public async Task<TruncateTestSession> CreateSessionAsync(
        DbDriver driver,
        string? schema,
        IEnumerable<Type> entityTypes,
        DbConnection? connection = null,
        CancellationToken cancellationToken = default)
    {
        var tables = entityTypes.Select(type => DapperHelper.GetEntityMapping(type).TableName)
            .Distinct().OrderBy(table => table, StringComparer.Ordinal);
        var acquired = new List<SemaphoreSlim>();
        TruncateTestSession? session = null;
        try
        {
            foreach (var table in tables)
            {
                var gate = _gates.GetOrAdd((driver.IsMySql() ? DbDriver.MySqlConnector : driver, schema, table),
                    _ => new SemaphoreSlim(1, 1));
                await gate.WaitAsync(cancellationToken);
                acquired.Add(gate);
            }
            session = new TruncateTestSession(connection ?? environment.Resolve(driver, schema).CreateConnection(),
                driver, schema, Release);
            await session.Connection.OpenAsync(cancellationToken);
            return session;
        }
        catch
        {
            if (session != null)
                session.Dispose();
            else
                Release();
            throw;
        }

        void Release()
        {
            foreach (var gate in acquired.AsEnumerable().Reverse())
                gate.Release();
        }
    }
}
