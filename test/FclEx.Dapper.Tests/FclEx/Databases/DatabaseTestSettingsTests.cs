namespace FclEx.Databases;

public class DatabaseTestSettingsTests
{
    [Fact]
    public void ExplicitSelection_IsCaseInsensitiveAndRemovesDuplicates()
    {
        Assert.Equal([DbDriver.Sqlite, DbDriver.Npgsql], SelectDrivers("sqlite, Npgsql, Sqlite", false, true));
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
        Assert.Contains(DbDriver.Npgsql, ProvisioningDrivers);
        Assert.Contains(DbDriver.MySqlConnector, ProvisioningDrivers);
        Assert.DoesNotContain(DbDriver.MySql, ProvisioningDrivers);
        Assert.DoesNotContain(DbDriver.Sqlite, ProvisioningDrivers);
    }
}
