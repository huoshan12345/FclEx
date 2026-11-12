namespace FclEx.Databases;

public class DatabaseTestSettingsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LocalDefaults_IncludeAllLocalEngines(bool isWindows)
    {
        var expected = new List<DbDriver>
        {
            DbDriver.SqlServer, 
            DbDriver.Sqlite, 
            DbDriver.MySqlConnector,
#if SUPPORT_NPGSQL
            DbDriver.Npgsql, 
#endif
#if SUPPORT_ORACLE
            DbDriver.Oracle,
#endif
        };
        Assert.Equal(expected, SelectDrivers(null, false, isWindows));
    }

    [Fact]
    public void ExplicitSelection_IsCaseInsensitiveAndRemovesDuplicates()
    {
        Assert.Equal([DbDriver.Sqlite, DbDriver.SqlServer], SelectDrivers("sqlite, SqlServer, Sqlite", false, true));
    }

    [Theory]
    [InlineData("")]
    [InlineData("unknown")]
    [InlineData("1")]
    public void InvalidSelection_ReportsConfigurationError(string selection)
    {
        Assert.Throws<ArgumentException>(() => SelectDrivers(selection, false, true));
    }

    [Fact]
    public void ProvisioningSelection_IsIndependentOfRunSelectionAndDeduplicatesMySqlEngine()
    {
        Assert.Contains(DbDriver.SqlServer, ProvisioningDrivers);
#if SUPPORT_NPGSQL
        Assert.Contains(DbDriver.Npgsql, ProvisioningDrivers);
#endif
        Assert.Contains(DbDriver.MySqlConnector, ProvisioningDrivers);
        Assert.DoesNotContain(DbDriver.MySql, ProvisioningDrivers);
        Assert.DoesNotContain(DbDriver.Sqlite, ProvisioningDrivers);
    }
}
