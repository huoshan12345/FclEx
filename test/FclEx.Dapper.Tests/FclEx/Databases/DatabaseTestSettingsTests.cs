namespace FclEx.Databases;

public class DatabaseTestSettingsTests
{
    [Theory]
    [InlineData(false, true, DbDriver.SqlServer)]
    [InlineData(false, false, DbDriver.SqlServer)]
    [InlineData(true, true, DbDriver.Npgsql)]
    [InlineData(true, false, DbDriver.SqlServer)]
    public void DefaultSelection_PreservesRemoteCoverageAndAddsSqlite(bool github, bool windows, DbDriver expected)
    {
        var drivers = SelectDrivers(null, github, windows);
        Assert.Contains(expected, drivers);
        Assert.Contains(DbDriver.Sqlite, drivers);
        Assert.Equal(drivers.Length, drivers.Distinct().Count());
    }

    [Fact]
    public void ExplicitSelection_IsCaseInsensitiveAndRemovesDuplicates()
        => Assert.Equal([DbDriver.Sqlite, DbDriver.Npgsql], SelectDrivers("sqlite, Npgsql, Sqlite", false, true));

    [Theory]
    [InlineData("")]
    [InlineData("Sqlite,")]
    [InlineData("unknown")]
    [InlineData("1")]
    public void InvalidSelection_ReportsConfigurationError(string selection)
        => Assert.Throws<ArgumentException>(() => SelectDrivers(selection, false, true));

    [Fact]
    public void ProvisioningSelection_IsIndependentOfRunSelectionAndDeduplicatesMySqlEngine()
    {
        Assert.Contains(DbDriver.SqlServer, ProvisioningDrivers);
        Assert.Contains(DbDriver.Npgsql, ProvisioningDrivers);
        Assert.Contains(DbDriver.MySqlConnector, ProvisioningDrivers);
        Assert.DoesNotContain(DbDriver.MySql, ProvisioningDrivers);
        Assert.DoesNotContain(DbDriver.Sqlite, ProvisioningDrivers);
    }
}
