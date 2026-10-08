using FclEx.Xunit;

namespace FclEx.Databases;

/// <summary>Shares truncate capability expectations and selected-driver cases between Dapper and EF tests.</summary>
public static class TruncateTestCases
{
    /// <summary>Reports whether the driver's truncate implementation accepts this identity/cascade combination.</summary>
    public static bool SupportsOptions(DbDriver driver, bool restartIdentity, bool cascade) => driver switch
    {
        DbDriver.Npgsql => true,
        DbDriver.Oracle => !restartIdentity,
        DbDriver.Sqlite => !cascade,
        _ => restartIdentity && !cascade,
    };

    /// <summary>Combines selected drivers and schemas with supported options for real-database result tests.</summary>
    public static TheoryData<DbDriver, string?, bool, bool> GetOptionCases(IEnumerable<string?> schemas)
        => (from pair in GetDriverSchemaCases(schemas)
            from restart in new[] { false, true }
            from cascade in new[] { false, true }
            where SupportsOptions(pair.Driver, restart, cascade)
            select (pair.Driver, pair.Schema, restart, cascade)).ToTheoryData();

    /// <summary>Lists unsupported options once per driver; rejection precedes connection opening and is independent of schema.</summary>
    public static TheoryData<DbDriver, bool, bool> UnsupportedOptionCases
    {
        get
        {
            var cases = (from driver in SelectedDrivers
                         from restart in new[] { false, true }
                         from cascade in new[] { false, true }
                         where !SupportsOptions(driver, restart, cascade)
                         select (driver, restart, cascade)).ToTheoryData();
            if (cases.Count == 0)
                cases.Add(new TheoryDataRow<DbDriver, bool, bool>(SelectedDrivers.FirstOrDefault(), false, true)
                {
                    Skip = "No selected driver has unsupported truncate options.",
                });
            return cases;
        }
    }

    /// <summary>Lists supported options for drivers whose truncate operation can participate in a rollback.</summary>
    /// <remarks>An explicitly skipped row is returned when the selection contains no such driver.</remarks>
    public static TheoryData<DbDriver, string?, bool> GetTransactionCases(IEnumerable<string?> schemas)
    {
        var cases = (from pair in GetDriverSchemaCases(schemas)
                     where pair.Driver is DbDriver.Sqlite or DbDriver.SqlServer or DbDriver.Npgsql
                     from restart in new[] { false, true }
                     where SupportsOptions(pair.Driver, restart, false)
                     select (pair.Driver, pair.Schema, restart)).ToTheoryData();
        if (cases.Count == 0)
            cases.Add(new TheoryDataRow<DbDriver, string?, bool>(SelectedDrivers.FirstOrDefault(), null, true)
            {
                Skip = "No selected driver supports rolling back TRUNCATE.",
            });
        return cases;
    }

    /// <summary>Lists PostgreSQL and Oracle targets for parent/child cascade result tests.</summary>
    /// <remarks>An explicitly skipped row is returned when neither driver is selected.</remarks>
    public static TheoryData<DbDriver, string?> GetCascadeCases(IEnumerable<string?> schemas)
    {
        var cases = GetDriverSchemaCases(schemas)
            .Where(pair => pair.Driver is DbDriver.Npgsql or DbDriver.Oracle)
            .Select(pair => (pair.Driver, pair.Schema)).ToTheoryData();
        if (cases.Count == 0)
            cases.Add(new TheoryDataRow<DbDriver, string?>(SelectedDrivers.FirstOrDefault(), null)
            {
                Skip = "No selected driver supports TRUNCATE CASCADE.",
            });
        return cases;
    }

    /// <summary>Selects SQLite for derived-connection regression tests, or returns an explicitly skipped row.</summary>
    public static TheoryData<DbDriver> SqliteDriverCases
    {
        get
        {
            var cases = SelectedDrivers.Where(driver => driver == DbDriver.Sqlite).ToTheoryData();
            if (cases.Count == 0)
                cases.Add(new TheoryDataRow<DbDriver>(DbDriver.Sqlite) { Skip = "SQLite is not selected." });
            return cases;
        }
    }
}
